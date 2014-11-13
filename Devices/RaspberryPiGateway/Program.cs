//  ---------------------------------------------------------------------------------
//  Copyright (c) Microsoft Open Technologies, Inc.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  ---------------------------------------------------------------------------------

//#define SIMULATEDATA
//#define LOG_MESSAGE_RATE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Trace = System.Diagnostics.Trace;
using System.IO.Ports;
using System.Text;
using System.Threading;

using Amqp;
using Amqp.Framing;
using Amqp.Types;

using Newtonsoft.Json;

namespace CloudPI
{
	class MainClass
	{
		public static int Main (string[] args)
		{
			var main = new MainClass ();

			int result = main.Parse (args);
			if (result != 0)
			{
				return result;
			}

			return main.Run ();
		}

#region Commandline Parameters

        string deviceId = Guid.NewGuid().ToString ("N"); // Unique identifier for the device
		string deviceDisplayName = "RPI001 C-LABS";   // Human readable name for the device

        // AMQP address string syntax is an example for Azure Service Bus/Event Hub
        // Any AMQP broker can be used by using the proper address/target
        Address address = new Address("amqps://<keyname>:<key>=@<namespace>.servicebus.windows.net");
        string target = "<eventhub name>";

        int sendFrequency = 1000;
        bool bForever = true;
#if !USE_WINDOWS_SERIAL_PORT
		string serialPortName ="/dev/ttyACM0";
#else
        string serialPortName ="COM10";
#endif
        SourceLevels traceLevel = SourceLevels.Error;
#endregion

        Dictionary<string, object> lastDataSample = null;

#if LOG_MESSAGE_RATE
        static int g_messageCount = 0;
#endif

		int Run ()
		{
            System.Diagnostics.Trace.Listeners.Add(new ConsoleTraceListener() 
            {
                TraceOutputOptions = TraceOptions.DateTime,
                Filter = new EventTypeFilter(traceLevel),
            });

            Amqp.Trace.TraceListener = (f, o) => Trace.WriteLine(String.Format(f, o));

			var sampleThread = new Thread (SampleLoop);
			sampleThread.Start ();

			Connection connection = null;
			Session session = null;
			SenderLink sender = null;

			do
			{
#if LOG_MESSAGE_RATE
                var stopWatch = Stopwatch.StartNew();
#endif
				try
				{
					connection = new Connection (address);
					session = new Session (connection);
					sender = new SenderLink (session, "send-link", target);

					for (int i = 0; bForever || i < 20; i++)
					{
						GetSampleAndSendAmqpMessage(sender);
                        Thread.Sleep (sendFrequency); // Wait until next sample
					}

					Thread.Sleep (10000);
				}
				catch (Exception e)
				{
					Trace.TraceError("Error connecting or sending message: {0}", e.Message);
				}
				finally
				{
					if (sender!=null) sender.Close ();
					if (session!=null) session.Close ();
					if (connection!=null) connection.Close ();
				}
				if (bForever)
				{
					Trace.TraceInformation("Restarting send loop...");
					Thread.Sleep (sendFrequency); // Wait until next sample
				}

			} while (bForever);
            MAINSWITCH = false;
			return 0;
		}

        void GetSampleAndSendAmqpMessage(SenderLink sender)
        {
            // sensor data is continuously sampled by a background thread
            // Obtain the last sample
            var sample = Interlocked.Exchange(ref lastDataSample, null);

            // No new sample since we checked last time: don't send
            if (sample == null) return;

            Message message = new Message();

            message.Properties = new Properties()
            {
                Subject = "wthr",              // Message type: Weather
                CreationTime = DateTime.UtcNow, // Time of data sampling
            };

            message.MessageAnnotations = new MessageAnnotations();
            // Event Hub partition key: device id - ensures that all messages from this device go to the same partition and thus preserve order/co-location at processing time
            message.MessageAnnotations[new Symbol("x-opt-partition-key")] = deviceId;

            message.ApplicationProperties = new ApplicationProperties();
            message.ApplicationProperties["time"] = message.Properties.CreationTime;
            message.ApplicationProperties["from"] = deviceId; // Originating device
            message.ApplicationProperties["dspl"] = deviceDisplayName;      // Display name for originating device

            if (sample != null && sample.Count > 0)
            {
#if! SENDAPPPROPERTIES
                var outDictionary = new Dictionary<string, object>(sample);
                outDictionary["Subject"] = message.Properties.Subject; // Message Type
                outDictionary["time"] = message.Properties.CreationTime;
                outDictionary["from"] = deviceId; // Originating device
                outDictionary["dspl"] = deviceDisplayName;      // Display name for originating device

                message.Properties.ContentType = "text/json";
                message.Body = new Data() { Binary = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(outDictionary)) };
#else
                foreach (var sampleProperty in sample)
				{
					message.ApplicationProperties [sample.Key] = sample.Value;
				}
#endif
            }
            else
            {
                message.Properties.Subject = "wthrerr";
            }

            // TODO Check if previous message has been sent. If not: cancel, so we optimize for sending the latest value always
            // Current code however provides maximum throughput (AMQP streaming without waiting for invididual ack)
            sender.Send(message, SendOutcome, null); // Send to the cloud asynchronously

#if LOG_MESSAGE_RATE
            if (g_messageCount >= 500)
            {
                float secondsElapsed = ((float)stopWatch.ElapsedMilliseconds) / (float)1000.0;
                if (secondsElapsed > 0)
                {
                    Console.WriteLine("Message rate: {0} msg/s", g_messageCount / secondsElapsed);
                    g_messageCount = 0;
                    stopWatch.Restart();
                }
            }
#endif
        }


		public static void SendOutcome (Message message, Outcome outcome, object state)
		{
			if (outcome is Accepted)
			{
				Trace.TraceInformation(String.Format ("Sent message {0} - {1}", message.ApplicationProperties ["time"], message.Properties.Subject));
#if LOG_MESSAGE_RATE
                g_messageCount++;
#endif
			}
			else
			{
				Trace.TraceError(String.Format ("Error sending message {0} - {1}, outcome {2}", message.ApplicationProperties ["time"], outcome, message.Properties.Subject));
			}
		}

        bool MAINSWITCH = true;

		public void SampleLoop ()
		{
			SerialPort serialPort = null;
			while (MAINSWITCH)
			{
				try
				{
#if !SIMULATEDATA
					Debug.WriteLine ("Opening Serial Port {0}", serialPortName);
					serialPort = new SerialPort (serialPortName, 9600);
					serialPort.DtrEnable = true;
					serialPort.Open ();
					Trace.TraceInformation ("Opened Serial Port {0}", serialPortName);
#endif
                    while (MAINSWITCH)
					{
#if! SIMULATEDATA
                        Debug.WriteLine ("Reading from Serial Port");
                        var valuesJson = serialPort.ReadLine ();
						Debug.WriteLine ("Read Data from Serial Port: {0}", valuesJson);
#else
						Random r = new Random ();
						string valuesJson = String.Format("{{ \"temp\" : {0}, \"hmdt\" : {1}, \"lght\" : {2}}}", 
						    (r.NextDouble() * 120) - 10,
						    (r.NextDouble() * 100),
						    (r.NextDouble() * 100));
#endif
						try
						{
							var valueDict = JsonConvert.DeserializeObject<Dictionary<string, object>> (valuesJson);
                            if (valueDict != null)
                            {
                                Interlocked.Exchange(ref lastDataSample, valueDict);

                                Trace.TraceInformation("Parsed data from serial port as: {0}", JsonConvert.SerializeObject(valueDict));
                            }
						}
						catch (Exception e)
						{
							Trace.TraceError("Error parsing data from serial port: {0}, Data: {1}", e.Message, valuesJson);
						}
						Thread.Sleep (Math.Min(100, sendFrequency));
					}
				}
				catch (Exception e)
				{
					Trace.TraceError("Error processing data from serial port: {0} ", e.Message);
					if (serialPort != null && serialPort.IsOpen)
					{
						Trace.TraceInformation("Closing Serial Port");
						serialPort.Close ();
						serialPort = null;
					}
					Thread.Sleep (800);
				}
			}
		}

		#region CommandLineParsing

		int Parse (string[] args)
		{
            Amqp.Trace.TraceLevel = Amqp.TraceLevel.Error; 

            bool bParseError = false;
			for (int i = 0; i < args.Length; i++)
			{
				switch (args [i].ToLowerInvariant ())
				{
					case "-forever":
						bForever = true;
						break;
					case "-tracelevel":
						bool bEndTraceLevel = false;
                        while (!bEndTraceLevel && ++i < args.Length)
                        {
                            switch (args [i].ToLowerInvariant ())
                            {
                                case "verbose":
                                    traceLevel |= SourceLevels.Verbose;
                                    break;
                                case "information":
                                    traceLevel |= SourceLevels.Information;
                                    break;
                                case "warning":
                                    traceLevel |= SourceLevels.Warning;
                                    break;
                                case "error":
                                    traceLevel |= SourceLevels.Error;
                                    break;
                                case "frame":
                                    Amqp.Trace.TraceLevel |= Amqp.TraceLevel.Frame;
                                    break;
                                default:
                                    bEndTraceLevel = true;
                                    i--;
                                    break;
                            }
                        }
						break;
					case "-address":
						i++;
						if (i < args.Length)
						{
							address = new Address (args [i]);
						}
						else
						{
							Console.WriteLine ("Error: missing address");
							bParseError = true;
						}
						break;
					case "-target":
						i++;
						if (i < args.Length)
						{
							target = args [i];
						}
						else
						{
							Console.WriteLine ("Error: missing target");
							bParseError = true;
						}
						break;
					case "-deviceid":
						i++;
						if (i < args.Length)
						{
							deviceId = args [i];
						}
						else
						{
							Console.WriteLine ("Error: missing device id");
							bParseError = true;
						}
						break;
					case "-devicename":
						i++;
						if (i < args.Length)
						{
							deviceDisplayName = args [i];
						}
						else
						{
							Console.WriteLine ("Error: missing device name");
							bParseError = true;
						}
						break;
					case "-serial":
						i++;
						if (i < args.Length)
						{
							serialPortName = args [i];
						}
						else
						{
							Console.WriteLine ("Error: missing serial port name");
							bParseError = true;
						}
						break;
					case "-frequency":
						i++;
						if (i < args.Length)
						{
							sendFrequency = Convert.ToInt32 (args [i]);
						}
						else
						{
							Console.WriteLine ("Error: missing frequency value");
							bParseError = true;
						}
						break;

					default:
						Console.WriteLine ("Error: unrecognized argument: {0}", args [i]);
						bParseError = true;
						break;
				}
			}

			if (bParseError)
			{
				Console.WriteLine ("Usage: CloudPI -forever | -tracelevel verbose information error frame warning | -deviceid <deviceid> | -devicename <devicename> | -serial <serial port name> | -frequency <frequency in ms> | -address <address> | -target <target>");
				return 1;
			}
			return 0;
		}

		#endregion
	}
}

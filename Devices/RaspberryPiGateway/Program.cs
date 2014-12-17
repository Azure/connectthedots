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

// #define DEBUG
// #define SIMULATEDATA
// #define LOG_MESSAGE_RATE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Threading;

using Amqp;
using Amqp.Framing;
using Amqp.Types;

using Newtonsoft.Json;

// include System.Configuration to read config file
using System.Configuration;
using System.Collections.Specialized;

// include NLog logging library. See https://github.com/NLog/NLog/wiki/Tutorial for information on adding NLog to project file and configuring NLog.conf
using NLog;


namespace RaspberryPiGateway
{

	class MainClass
	{
        // Set up config file reads
        public static string AppSubject;
        public static string AppSensor;
        public static string AppEdgeGateway;
        public static Address AppAMQPAddress;
        public static string sAppAMQPAddress;
        public static string AppEHTarget;
        public static string AppDeviceDisplayName;
        public static Int32 sendFrequency;
        public static bool bForever;
        public static string serialPortName;
        public static string AppKey1;
        public static string AppKey2;
        public static string AppKey3;


        // Set up logging
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        
		public static int Main (string[] args)
		{

            var main = new MainClass ();

            // Open RaspberryPiGateway.exe.config file for device information
            try
            {
                string applicationName = "RaspberryPiGateway.exe";
                string exePath = System.IO.Path.Combine(Environment.CurrentDirectory, applicationName);
                var configFile = ConfigurationManager.OpenExeConfiguration(exePath);
                var appSettings = ConfigurationManager.AppSettings;
                if (appSettings.Count == 0)
                {
                    logger.Info("AppSettings is empty.");
                }
                else
                {
                    foreach (var key in appSettings.AllKeys)
                    {
                        logger.Info("Key: {0} Value: {1}", key, appSettings[key]);
                    }
                }
            }
            catch (ConfigurationErrorsException)
            {
                logger.Info("Error reading app settings");
            }

            // Read a particular key from the config file
            AppSubject = ConfigurationManager.AppSettings.Get("Subject");
            AppSensor = ConfigurationManager.AppSettings.Get("Sensor");
            AppEdgeGateway = ConfigurationManager.AppSettings.Get("EdgeGateway");
            AppDeviceDisplayName = ConfigurationManager.AppSettings.Get("DeviceDisplayName");
            sAppAMQPAddress = ConfigurationManager.AppSettings.Get("AMQPAddress");
            AppAMQPAddress = new Address(sAppAMQPAddress);
            AppEHTarget = ConfigurationManager.AppSettings.Get("EHTarget");
            sendFrequency = Convert.ToInt32(ConfigurationManager.AppSettings.Get("SendFrequency"));
            serialPortName = ConfigurationManager.AppSettings.Get("serialPortName");
            bForever = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("Forever"));
            AppKey1 = ConfigurationManager.AppSettings.Get("Key1");
            AppKey2 = ConfigurationManager.AppSettings.Get("Key2");
            AppKey3 = ConfigurationManager.AppSettings.Get("Key3");

            int result = main.Parse (args);
			if (result != 0)
			{
				return result;
			}
			return main.Run ();
		}

#region Commandline Parameters

        string deviceId = Guid.NewGuid().ToString ("N"); // Unique identifier for the device

#if USE_WINDOWS_SERIAL_PORT
        string serialPortName ="COM10";
#endif

#endregion

        Dictionary<string, object> lastDataSample = null;

#if LOG_MESSAGE_RATE
        static int g_messageCount = 0;
#endif

		int Run ()
		{

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
					connection = new Connection (AppAMQPAddress);
					session = new Session (connection);
					sender = new SenderLink (session, "send-link", AppEHTarget);

					for (int i = 0; bForever || i < 20; i++)
					{
						GetSampleAndSendAmqpMessage(sender);
                        Thread.Sleep (sendFrequency); // Wait until next sample
					}

					Thread.Sleep (10000);
				}
				catch (Exception e)
				{
                    logger.Error("Error connecting or sending message: {0}", e.Message);
                }
				finally
				{
					if (sender!=null) sender.Close ();
					if (session!=null) session.Close ();
					if (connection!=null) connection.Close ();
				}
				if (bForever)
				{
#if DEBUG
                    logger.Info("Restarting send loop...");
#endif
					Thread.Sleep (sendFrequency); // Wait until next sample
				}

			} while (bForever);
            MAINSWITCH = false;
			return 0;
		}

        void GetSampleAndSendAmqpMessage(SenderLink sender)
        {
            // Obtain the last sample as gathered by the background thread 
            // Interlocked.Exchange guarantees that changes are done atomically between main and background thread
            var sample = Interlocked.Exchange(ref lastDataSample, null);

            // No new sample since we checked last time: don't send anything
            if (sample == null) return;

            Message message = new Message();

            message.Properties = new Properties()
            {
                Subject = AppSubject,              // Message type defined in App.config file for sensor
                CreationTime = DateTime.UtcNow, // Time of data sampling
            };

            message.MessageAnnotations = new MessageAnnotations();
            // Event Hub partition key: device id - ensures that all messages from this device go to the same partition and thus preserve order/co-location at processing time
            message.MessageAnnotations[new Symbol("x-opt-partition-key")] = deviceId;
            message.ApplicationProperties = new ApplicationProperties();
            message.ApplicationProperties["time"] = message.Properties.CreationTime;
            message.ApplicationProperties["from"] = deviceId; // Originating device
            message.ApplicationProperties["dspl"] = AppDeviceDisplayName;      // Display name for originating device

            if (sample != null && sample.Count > 0)
            {
#if! SENDAPPPROPERTIES
                var outDictionary = new Dictionary<string, object>(sample);
                outDictionary["Subject"] = message.Properties.Subject; // Message Type
                outDictionary["time"] = message.Properties.CreationTime;
                outDictionary["from"] = deviceId; // Originating device
                outDictionary["dspl"] = AppDeviceDisplayName;      // Display name for originating device
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
                // No data: send an empty message with message type "weather error" to help diagnose problems "from the cloud"
                message.Properties.Subject = "wthrerr";
            }

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
//#if DEBUG
                logger.Info("Sent message from {0} {1} at {2}", AppEdgeGateway, AppSensor, message.ApplicationProperties["time"]);
//#endif
#if LOG_MESSAGE_RATE
                g_messageCount++;
#endif
			}
			else
			{
                logger.Error("Error sending message {0} - {1}, outcome {2}", message.ApplicationProperties["time"],  message.Properties.Subject,outcome);
                logger.Error("Error sending to {0} at {1}", AppEHTarget, AppAMQPAddress);
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
#if DEBUG
                    logger.Info("Opened Serial Port {0}", serialPortName);
#endif
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
//#if DEBUG
                                logger.Info("Parsed data from serial port on {0} as: {1}", AppDeviceDisplayName, JsonConvert.SerializeObject(valueDict));
//#endif
                            }
						}
						catch (Exception e)
						{
                            logger.Error("Error parsing data from serial port: {0}, Data: {1}", e.Message, valuesJson);
						}
						Thread.Sleep (Math.Min(100, sendFrequency));
					}
				}
				catch (Exception e)
				{
                    logger.Error("Error processing data from serial port: {0} ", e.Message);
					if (serialPort != null && serialPort.IsOpen)
					{
#if DEBUG
                        logger.Info("Closing Serial Port");
#endif
						serialPort.Close ();
						serialPort = null;
					}
					Thread.Sleep (800);
				}
			}
		}

		#region CommandLineParsing

        // not used in current sample code, but portion left in, commented out, for easy re-implementation
		int Parse (string[] args)
		{

            bool bParseError = false;
			for (int i = 0; i < args.Length; i++)
			{
				switch (args [i].ToLowerInvariant ())
				{
				//	case "-serial":
				//		i++;
				//		if (i < args.Length)
				//		{
				//			serialPortName = args [i];
				//		}
				//		else
				//		{
				//			Console.WriteLine ("Error: missing serial port name");
				//			bParseError = true;
				//		}
				//		break;
				//	default:
				//		Console.WriteLine ("Error: unrecognized argument: {0}", args [i]);
				//		bParseError = true;
				//		break;
				}
			}

			if (bParseError)
			{
                // Provide guidance on correct autorun.sh command line if necessary, e.g
                //Console.WriteLine("Usage: RaspberryPiGateway -serial <serial port name> ");
                Console.WriteLine("Error: parsing error");
 				return 1;
			}
			return 0;
		}

		#endregion
	}
}

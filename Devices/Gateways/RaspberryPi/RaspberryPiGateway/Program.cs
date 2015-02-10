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
using System.IO;
using System.Net;

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
    /// <summary>
    /// Class used to list the running threads listening to Serial Ports
    /// </summary>
    class SerialPortListeningThread
    {
        public SerialPortListeningThread(string name, Thread thread)
        {
            portName = name;
            listeningThread = thread;
        }
        public string portName { get; set; }
        public Thread listeningThread { get; set; }
    }

    /// <summary>
    /// Main class of the application
    /// </summary>
    class MainClass
    {
        // Below parameters are read from the app config file
        public static string AppEdgeGateway;
        public static Address AppAMQPAddress;
        public static string sAppAMQPAddress;
        public static string AppEHTarget;

        // Unique identifier for the gateway (will be generated when app starts)
        // You might want to hard code it or put this ID in the configuration file if you need the Gateway to not change ID at each reboot
        public static string deviceId;
        public static string deviceIP;

        // Variables for AMQPs connection
        public static Connection connection = null;
        public static Session session = null;
        public static SenderLink sender = null;
        // We have several threads that will use the same SenderLink object
        // we will protect the access using InterLock.Exchange 0 for false, 1 for true. 
        private static int sendingMessage = 0;

        // Below variables will be used to detect and work with the serial ports (sensors boards connected to the Gateway)
        public static List<SerialPortListeningThread> listeningThreads = new List<SerialPortListeningThread>();

        // Set up logging
        public static Logger logger = NLog.LogManager.GetCurrentClassLogger();
#if LOG_MESSAGE_RATE
        static int g_messageCount = 0;
#endif

        // boolean switch used to manage threads accross the app
        public static bool MAINSWITCH = true;

        /// <summary>
        /// Main function of the application
        /// </summary>
        /// <param name="args"></param>
        /// <returns>
        /// -1 when an error occured during initialization
        /// result of the run function which is a loop returning 0 when ended
        /// </returns>
        public static int Main(string[] args)
        {
            var main = new MainClass();

            // Initialize application by reading configuration file
            if (!InitAppSettings(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "RaspberryPiGateway.exe")) return -1;
            // Initializa AMQP connection
            if (!InitAMQPConnection(false)) return -1;

            // Get IP address of gateway
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            deviceIP = ipAddress.ToString();

            // start main routine
            return main.Run();
        }

        /// <summary>
        /// Initialize AMQP connection
        /// we are using the connection to send data to Azure Event Hubs
        /// Connection information is retreived from the app configuration file
        /// </summary>
        /// <returns>
        /// true when successful
        /// false when unsuccessful
        /// </returns>
        public static bool InitAMQPConnection(bool reset)
        {
            if (reset)
            {
                // If the reset flag is set, we need to kill previous connection 
                try
                {
                    logger.Info("Resetting connection to Azure Event Hub");
                    logger.Info("Closing any existing senderLink, session and connection.");
                    if (sender != null) sender.Close();
                    if (session != null) session.Close();
                    if (connection != null) connection.Close();
                }
                catch (Exception e)
                {
                    logger.Error("Error closing AMQP connection to Azure Event Hub: {0}", e.Message);
                }
            }

            logger.Info("Initializing connection to Azure Event Hub");
            // Initialize AMQPS connection
            try
            {
                connection = new Connection(AppAMQPAddress);
                session = new Session(connection);
                sender = new SenderLink(session, "send-link", AppEHTarget);
            }
            catch (Exception e)
            {
                logger.Error("Error connecting to Azure Event Hub: {0}", e.Message);
                if (sender != null) sender.Close();
                if (session != null) session.Close();
                if (connection != null) connection.Close();
                return false;
            }
            logger.Info("Connection to Azure Event Hub initialized.");
            return true;
        }

        /// <summary>
        /// Get application settings from configuration file
        /// the configuration file contains information for the Gateway to connect to Azure Event Hubs.
        /// </summary>
        /// <param name="currentDirectory">Current directory the app is running in</param>
        /// <param name="applicationName">Name of the application</param>
        /// <returns>
        /// true when successful
        /// false when unsuccessful
        /// </returns>
        public static bool InitAppSettings(string currentDirectory, string applicationName)
        {
            // Open RaspberryPiGateway.exe.config file for device information
            try
            {
                logger.Info("Trying to open configuration file.");
                logger.Info("Current Directory {0}", currentDirectory);
                logger.Info("Application Name {0}", applicationName);
                string exePath = System.IO.Path.Combine(currentDirectory, applicationName);
                var configFile = ConfigurationManager.OpenExeConfiguration(exePath);
                var appSettings = ConfigurationManager.AppSettings;
                if (appSettings.Count == 0)
                {
                    // We cannot run without the connection parameters from 
                    logger.Info("AppSettings is empty.");
                    return false;
                }
                else
                {
                    foreach (var key in appSettings.AllKeys)
                    {
                        logger.Info("Gateway config file key: {0} Value: {1}", key, appSettings[key]);
                    }
                    // Read relevant keys from the config file and store in a variable
                    AppEdgeGateway = ConfigurationManager.AppSettings.Get("EdgeGateway");
                    sAppAMQPAddress = ConfigurationManager.AppSettings.Get("AMQPAddress");
                    AppAMQPAddress = new Address(sAppAMQPAddress);
                    AppEHTarget = ConfigurationManager.AppSettings.Get("EHTarget");
                    // All settings retreived, we can return
                    return true;
                }
            }
            catch (ConfigurationErrorsException e)
            {
                logger.Error("Error reading app settings: {0}{1}", e.Message, e.InnerException.Message);
            }

            // Something didn't go right...
            return false;
        }

        /// <summary>
        /// Main thread of the application
        /// The Thread is monitoring serial ports and loops every 5 seconds
        /// When a new COM port is available, it starts a new listening thread
        /// When a COM port is no longer available, it kills the corresponding thread that has been created previously
        /// </summary>
        /// <returns>
        /// 0 when thread ends
        /// </returns>
        public int Run()
        {

#if LOG_MESSAGE_RATE
                var stopWatch = Stopwatch.StartNew();
#endif
            do
            {
                // We will monitor available COM ports and create listening thread for each new valid port
#if !SIMULATEDATA
                // Identify which serial ports are connected to sensors
                var ports = GetPortNames();

                // First we make sure we kill listening threads for COM port that are no longer available
                var threadsKilled = new List<SerialPortListeningThread>();
                foreach (SerialPortListeningThread serialPortThread in listeningThreads)
                {
                    if (Array.IndexOf(ports, serialPortThread.portName) == -1)
                    {
                        // Serial port is no longer valid. Abort the listening process
                        serialPortThread.listeningThread.Abort();
                        threadsKilled.Add(serialPortThread);
                    }
                }

                // we cannot remove a list item in a foreach loop
                foreach (SerialPortListeningThread threadKilled in threadsKilled)
                {
                    listeningThreads.Remove(threadKilled);
                }

                // For each of the valid serial ports, start a new listening thread if not already created
                foreach (string serialPortName in ports)
                {
                    if (!listeningThreads.Exists(x => x.portName.Equals(serialPortName))
                        )
                    {
                        logger.Info("Found serial port with Normal attribute: {0}", serialPortName);

                        // Start a listening thread for each serial port
                        var listeningThread = new Thread(() => ListeningForSensors(serialPortName));
                        listeningThread.Start();
                        listeningThreads.Add(new SerialPortListeningThread(serialPortName, listeningThread));
                    }
                }

                // If we have no serial port connect, log it
                if (listeningThreads.Count == 0)
                {
                    logger.Error("No connected serial ports");
                }
#else
                if (listeningThreads.Count == 0)
                {
                    // Start a unique thread simulating data
                    var listeningThread = new Thread(() => ListeningForSensors("Simulated"));
                    listeningThread.Start();
                    listeningThreads.Add(new SerialPortListeningThread("Simulated", listeningThread));
                }
#endif
                // Every 5 seconds we scan Serial COM ports
                Thread.Sleep(5000);
            } while (MAINSWITCH);
            return 0;
        }

        /// <summary>
        /// Send a string as an AMQP message to Azure Event Hub
        /// </summary>
        /// <param name="valuesJson">
        /// String to be sent as an AMQP message to Event Hub
        /// </param>
        public static void SendAmqpMessage(string valuesJson)
        {
            Message message = new Message();

            // If there is no value passed as parameter, do nothing
            if (valuesJson == null) return;

            try
            {
                // Deserialize Json message
                var sample = JsonConvert.DeserializeObject<Dictionary<string, object>>(valuesJson);
                if (sample == null)
                {
                    logger.Info("Error parsing JSON message {0}", valuesJson);
                    return;
                }
#if DEBUG
                logger.Info("Parsed data from serial port: {0}", valuesJson);
#endif

                // Convert JSON data in 'sample' into body of AMQP message
                // Only data added by gateway is time of message (since sensor may not have clock) 
                deviceId = Convert.ToString(sample["DeviceGUID"]);      // Unique identifier from sensor, to group items in event hub

                message.Properties = new Properties()
                {
                    Subject = Convert.ToString(sample["Subject"]),              // Message type (e.g. "wthr") defined in sensor code, sent in JSON payload
                    CreationTime = DateTime.UtcNow, // Time of data sampling
                };

                message.MessageAnnotations = new MessageAnnotations();
                // Event Hub partition key: device id - ensures that all messages from this device go to the same partition and thus preserve order/co-location at processing time
                message.MessageAnnotations[new Symbol("x-opt-partition-key")] = deviceId;
                message.ApplicationProperties = new ApplicationProperties();
                message.ApplicationProperties["time"] = message.Properties.CreationTime;
                message.ApplicationProperties["from"] = deviceId; // Originating device
                message.ApplicationProperties["dspl"] = sample["dspl"] + " (" + deviceIP + ")";      // Display name for originating device defined in sensor code, sent in JSON payload

                if (sample != null && sample.Count > 0)
                {
#if! SENDAPPPROPERTIES

                    var outDictionary = new Dictionary<string, object>(sample);
                    outDictionary["Subject"] = message.Properties.Subject; // Message Type
                    outDictionary["time"] = message.Properties.CreationTime;
                    outDictionary["from"] = deviceId; // Originating device
                    outDictionary["dspl"] = sample["dspl"] + " (" + deviceIP + ")";      // Display name for originating device
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
            }
            catch (Exception e)
            {
                logger.Error("Error when deserializing JSON data received over serial port: {0}", e.Message);
                return;
            }

            // Send to the cloud asynchronously
            // Obtain handle on AMQP sender-link object
            if (0 == Interlocked.Exchange(ref sendingMessage, 1))
            {
                bool AMQPConnectionIssue = false;
                try
                {
                    // Message send function is asynchronous, we will receive completion info in the SendOutcome function
                    sender.Send(message, SendOutcome, null);
                }
                catch (Exception e)
                {
                    // Something went wrong let's try and reset the AMQP connection
                    logger.Error("Exception while sending AMQP message: {1}", e.Message);
                    AMQPConnectionIssue = true;
                }
                Interlocked.Exchange(ref sendingMessage, 0);

                // If there was an issue with the AMQP connection, try to reset it
                while (AMQPConnectionIssue)
                {
                    AMQPConnectionIssue = !InitAMQPConnection(true);
                    Thread.Sleep(200);
                }
            }

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

        /// <summary>
        /// Callback function used to report on AMQP message send 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="outcome"></param>
        /// <param name="state"></param>
        public static void SendOutcome(Message message, Outcome outcome, object state)
        {
            if (outcome is Accepted)
            {
//#if DEBUG
                logger.Info("Sent message from {0} at {1}", AppEdgeGateway, message.ApplicationProperties["time"]);
//#endif
#if LOG_MESSAGE_RATE
                g_messageCount++;
#endif
            }
            else
            {
                logger.Error("Error sending message {0} - {1}, outcome {2}", message.ApplicationProperties["time"], message.Properties.Subject, outcome);
                logger.Error("Error sending to {0} at {1}", AppEHTarget, AppAMQPAddress);
            }
        }

        /// <summary>
        /// Thread function for listening on a specific COM port for a JSON message.
        /// When a new message is received, send it directly to Azure Event Hubs using the SendAMQPMessage function
        /// </summary>
        /// <param name="port">COM port name to listen on</param>
        public static void ListeningForSensors(string port)
        {
            string serialPortName = port;
            SerialPort serialPort = null;
            bool serialPortAlive = true;

            // We want the thread to restart listening on the serial port if it crashed
            while (MAINSWITCH)
            {
                logger.Info("Starting listening loop for serial port {0}", serialPortName);
                try
                {
#if !SIMULATEDATA
                    serialPort = new SerialPort(serialPortName, 9600);
                    serialPort.DtrEnable = true;
                    serialPort.Open();
                    logger.Info("Opened Serial Port {0}", serialPortName);
#endif
                    do
                    {
                        // When simulating data, we will generate random data
                        // when not simulating, we read the serial port
                        string valuesJson = "";
#if! SIMULATEDATA
                        try
                        {
                            valuesJson = serialPort.ReadLine();
                        }
                        catch (Exception e)
                        {
                            logger.Error("Error Reading from Serial Portand sending data from serial port {0}: {1}", serialPortName, e.Message);
                            serialPort.Close();
                            serialPortAlive = false;
                        }
#else
						Random r = new Random ();
                        valuesJson = String.Format("{{ \"temp\" : {0}, \"hmdt\" : {1}, \"lght\" : {2}, \"DeviceGUID\" : \"{3}\", \"Subject\" : \"{4}\", \"dspl\" : \"{5}\"}}", 
						    (r.NextDouble() * 120) - 10,
						    (r.NextDouble() * 100),
						    (r.NextDouble() * 100),
                            "81E79059-A393-4797-8A7E-526C3EF9D64B",
                            "wthr",
                            "Simulator");
#endif

                        if (serialPortAlive)
                        {
                            try
                            {
                                // Send JSON message to the Cloud
                                SendAmqpMessage(valuesJson);
                            }
                            catch (Exception e)
                            {
                                logger.Error("Error sending AMQP data: {0}", e.Message);
                            }
                        }
                    } while (serialPortAlive);

                }
                catch (Exception e)
                {
                    logger.Error("Error processing data from serial port: {0} ", e.Message);
                }

                // When we are reaching this point, that means whether the COM port reading failled or the sensors has been disconnected
                // we will try to close the port properly, but if the device has been disconnected, this will trigger an exception
                try
                {
                    if (serialPort.IsOpen) serialPort.Close();
                    if (serialPort != null) serialPort = null;
                }
                catch (Exception e)
                {
                    logger.Error("Error when trying to close the serial port: {0} ", e.Message);
                }
                // We restart the thread if there has been some failure when reading from serial port
                Thread.Sleep(800);
            }
        }

        /// <summary>
        /// GetPortNames is redefined to support Unix systems as well.
        /// On Windows it will just use the SerialPort.GetPortNames function
        /// On Unix systems it will parse IO Files and look for the ones with names containing tty*
        /// </summary>
        /// <returns>
        /// Array of strings with the list of available COM ports
        /// </returns>
        private static string[] GetPortNames()
        {
            int p = (int)Environment.OSVersion.Platform;
            List<string> serial_ports = new List<string>();

            // Are we on Unix?
            if (p == 4 || p == 128 || p == 6)
            {
                string[] ttys = System.IO.Directory.GetFiles("/dev/", "tty*");
                foreach (string dev in ttys)
                {
                    //Arduino MEGAs show up as ttyACM due to their different USB<->RS232 chips
                    if (dev.StartsWith("/dev/ttyS") || dev.StartsWith("/dev/ttyUSB") || dev.StartsWith("/dev/ttyACM"))
                    {
                        serial_ports.Add(dev);
                    }
                }
            }
            else
            {
                serial_ports.AddRange(SerialPort.GetPortNames());
            }

            return serial_ports.ToArray();
        }
    }
}

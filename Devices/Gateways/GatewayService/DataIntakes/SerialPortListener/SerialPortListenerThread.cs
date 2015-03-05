//#define SIMULATEDATA


namespace SerialPortListener
{
    using System;
    using System.Collections.Generic;
    using System.IO.Ports;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ConnectTheDots.Common;
    using Microsoft.ConnectTheDots.Gateway;

    //--//

    public class SerialPortListenerThread : DataIntakeAbstract
    {
        internal class SerialPortListeningThread
        {
            public SerialPortListeningThread(string name, Thread thread)
            {
                portName = name;
                listeningThread = thread;
            }
            public string portName { get; set; }
            public Thread listeningThread { get; set; }
        }

        const int SLEEP_TIME_BETWEEN_SCAN = 5000; // 5 sec

        private readonly List<SerialPortListeningThread> _ListeningThreads = new List<SerialPortListeningThread>();

        private Func<string, int> _Enqueue;
        private bool _DoWorkSwitch;

        public SerialPortListenerThread( ILogger logger )
            : base( logger ) 
        {
        }

        public override bool Start( Func<string, int> enqueue )
        {
            _Enqueue = enqueue;

            _DoWorkSwitch = true;

            var sh = new SafeAction<int>( ( t ) => RunForSerial( t ), _Logger ); 

            Task.Run( ( ) => sh.SafeInvoke( SLEEP_TIME_BETWEEN_SCAN ) );

            return true;
        }

        public override bool Stop( )
        {
            _DoWorkSwitch = false;

            return true;
        }

        public override bool SetEndpoint(SensorEndpoint endpoint)
        {
            //we don't need any endpoints for this Data Intake
            if (endpoint == null)
                return true;

            return false;
        }

        public int RunForSerial( int scanPeriod )
        {
#if LOG_MESSAGE_RATE
            var stopWatch = Stopwatch.StartNew();
#endif
            do
            {
                _Logger.LogInfo("RunForSerial loop for Serial Ports scan.");
                // We will monitor available COM ports and create listening thread for each new valid port
#if !SIMULATEDATA
                // Identify which serial ports are connected to sensors
                var ports = GetPortNames();

                // First we make sure we kill listening threads for COM port that are no longer available
                var threadsKilled = new List<SerialPortListeningThread>();
                foreach (SerialPortListeningThread serialPortThread in _ListeningThreads)
                {
                    if (Array.IndexOf(ports, serialPortThread.portName) == -1)
                    {
                        // Serial port is no longer valid. Abort the listening process
                        _Logger.LogInfo("Killed serial port: " + serialPortThread.portName);
                        serialPortThread.listeningThread.Abort();
                        threadsKilled.Add(serialPortThread);
                    }
                }

                // we cannot remove a list item in a foreach loop
                foreach (SerialPortListeningThread threadKilled in threadsKilled)
                {
                    _ListeningThreads.Remove(threadKilled);
                }

                // For each of the valid serial ports, start a new listening thread if not already created
                foreach (string serialPortName in ports)
                {
                    if (!_ListeningThreads.Exists(x => x.portName.Equals(serialPortName)))
                    {
                        _Logger.LogInfo("Found serial port with Normal attribute: " + serialPortName);

                        // Start a listening thread for each serial port
                        string name = serialPortName;
                        var listeningThread = new Thread(() => ListeningForSensors(name));
                        listeningThread.Start();
                        _ListeningThreads.Add(new SerialPortListeningThread(serialPortName, listeningThread));
                    }
                }

                // If we have no serial port connect, log it
                if (_ListeningThreads.Count == 0)
                {
                    _Logger.LogError("No connected serial ports");
                }
#else
                if (_ListeningThreads.Count == 0)
                {
                    // Start a unique thread simulating data
                    var listeningThread = new Thread(() => ListeningForSensors("Simulated"));
                    listeningThread.Start();
                    _ListeningThreads.Add(new SerialPortListeningThread("Simulated", listeningThread));
                }
#endif
                // Every 5 seconds we scan Serial COM ports
                Thread.Sleep( scanPeriod );
            } while (_DoWorkSwitch);
            return 0;
        }

        /// <summary>
        /// Thread function for listening on a specific COM port for a JSON message.
        /// When a new message is received, send it directly to Azure Event Hubs using the SendAMQPMessage function
        /// </summary>
        /// <param name="port">COM port name to listen on</param>
        public void ListeningForSensors(string port)
        {
            _Logger.LogInfo("ListeningForSensors: " + port);
            string serialPortName = port;
            SerialPort serialPort = null;
            bool serialPortAlive = true;
            // We want the thread to restart listening on the serial port if it crashed
            while (_DoWorkSwitch)
            {
                _Logger.LogInfo("Starting listening loop for serial port " + serialPortName);

                try
                {
#if !SIMULATEDATA
                    serialPort = new SerialPort(serialPortName, 9600);
                    serialPort.DtrEnable = true;
                    serialPort.Open();
                
                    _Logger.LogInfo("Opened Serial Port " + serialPortName);
#endif
                    do
                    {
                        // When simulating data, we will generate random data
                        // when not simulating, we read the serial port
                        string valuesJson = "";
#if !SIMULATEDATA
                        try
                        {
                            valuesJson = serialPort.ReadLine();
                        }
                        catch (Exception e)
                        {
                            _Logger.LogError("Error Reading from Serial Portand sending data from serial port " + serialPortName + ":" + e.Message);

                            serialPort.Close();
                            serialPortAlive = false;
                        }
#else
                        Random r = new Random ();
                        valuesJson = String.Format("{{ \"temp\" : {0}, \"hmdt\" : {1}, \"lght\" : {2}}}", 
                            (r.NextDouble() * 120) - 10,
                            (r.NextDouble() * 100),
                            (r.NextDouble() * 100));
#endif

                        if (serialPortAlive)
                        {
                            try
                            {
                                // Show serialPort string that will be sent via AMQP
                                //_Logger.Info(valuesJson);

                                // Send JSON message to the Cloud
                                _Enqueue(valuesJson);
                            }
                            catch (Exception e)
                            {
                                _Logger.LogError("Error sending AMQP data: " + e.Message);
                            }
                        }
                    } while (serialPortAlive);

                }
                catch (Exception e)
                {
                    _Logger.LogError("Error processing data from serial port: " + e.Message);
                }

                // When we are reaching this point, that means whether the COM port reading failled or the sensors has been disconnected
                // we will try to close the port properly, but if the device has been disconnected, this will trigger an exception
                try
                {
                    if (serialPort != null)
                    {
                        if( serialPort.IsOpen )
                        {
                            serialPort.Close( );
                        }

                        serialPort = null;
                    }
                }
                catch (Exception e)
                {
                    _Logger.LogError("Error when trying to close the serial port: " + e.Message);
                }
                // We restart the thread if there has been some failure when reading from serial port
                Thread.Sleep(800);
            }
        }

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

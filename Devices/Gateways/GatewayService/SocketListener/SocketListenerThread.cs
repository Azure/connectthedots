using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Gateway.DataIntake;
using Gateway.Utils.Logger;

namespace SocketListener
{
    public class SocketListenerThread : IDataIntake
    {
        const int CONNECTION_RETRIES = 20;
        const int SLEEP_TIME_BETWEEN_RETRIES = 1000; // 1 sec

        private static ILogger _Logger;
        private static Func<string, int> _Enqueue;
        private static Func<bool> _DoWorkSwitch;

        private Thread listeningThread = null;

        public bool Start(Func<string, int> enqueue, ILogger logger, Func<bool> doWorkSwitch)
        {
            _Enqueue = enqueue;
            _Logger = logger;
            _DoWorkSwitch = doWorkSwitch;

            Task.Run(() => RunForSocket());
            return true;
        }

        public int RunForSocket()
        {
            int step = CONNECTION_RETRIES;

            Socket client = null;
            while (--step > 0 && _DoWorkSwitch())
            {
                try
                {
                    if (_Logger != null)
                        _Logger.LogInfo("Try connecting to device - step: " + (CONNECTION_RETRIES - step));

                    IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                    IPAddress ipAddress = ipHostInfo.AddressList[0];
                    IPEndPoint remoteEP = new IPEndPoint(ipAddress, 5000);

                    client = new Socket(
                        AddressFamily.InterNetwork,
                        SocketType.Stream,
                        ProtocolType.Tcp);
                    client.Connect(remoteEP);

                    if (client.Connected)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (_Logger != null)
                    {
                        _Logger.LogError("Exception when opening socket:" + ex.StackTrace);
                        _Logger.LogError("Will retry in 1 second");
                    }
                }

                // wait and try again
                Thread.Sleep(SLEEP_TIME_BETWEEN_RETRIES);
            }

            if (client != null && client.Connected)
            {
                if (_Logger != null)
                    _Logger.LogInfo(string.Format("Socket connected to {0}", client.RemoteEndPoint.ToString()));

                listeningThread = new Thread(() => SensorDataClient(client));
                listeningThread.Start();

                if (_Logger != null)
                    _Logger.LogInfo(string.Format("Reader thread started"));
                listeningThread.Join();
                if (_Logger != null)
                    _Logger.LogInfo("Listening thread terminated. Quitting.");
            }
            else
            {
                if (_Logger != null)
                    _Logger.LogError("No sensor connection detected. Quitting.");
            }
            return 0;
        }

        public static void SensorDataClient(Socket client)
        {
            try
            {
                _Logger.LogError("SensorDataClient");    
                StringBuilder jsonBuilder = new StringBuilder();
                byte[] buffer = new Byte[1024];
                // Use Regular Expressions (Regex) to parse incoming data, which may contain multiple JSON strings 
                // USBSPLSOCKET.PY uses "<" and ">" to terminate JSON string at each end, so built Regex to find strings surrounded by angle brackets
                // You can test Regex extractor against a known string using a variety of online tools, such as http://regexhero.net/tester/ for C#.
                //Regex dataExtractor = new Regex(@"<(\d+.?\d*)>");
                Regex dataExtractor = new Regex("<([\\w\\s\\d:\",-{}.]+)>");

                while (_DoWorkSwitch())
                {
                    try
                    {
                        int bytesRec = client.Receive(buffer);
                        int matchCount = 1;
                        // Read string from buffer
                        string data = Encoding.ASCII.GetString(buffer, 0, bytesRec);
                        //logger.Info("Read string: " + data);
                        if (data.Length > 0)
                        {
                            // Parse string into angle bracket surrounded JSON strings
                            var matches = dataExtractor.Matches(data);
                            if (matches.Count >= 1)
                            {
                                foreach (Match m in matches)
                                {
                                    jsonBuilder.Clear();
                                    // Remove angle brackets
                                    //jsonBuilder.Append("{\"dspl\":\"Wensn Digital Sound Level Meter\",\"Subject\":\"sound\",\"DeviceGUID\":\"81E79059-A393-4797-8A7E-526C3EF9D64B\",\"decibels\":");
                                    jsonBuilder.Append(m.Captures[0].Value.Trim().Substring(1, m.Captures[0].Value.Trim().Length - 2));
                                    //jsonBuilder.Append("}");
                                    string jsonString = jsonBuilder.ToString();
                                    //logger.Info("About to call SendAMQPMessage with JSON string: " + jsonString);
                                    _Enqueue(jsonString);
                                    matchCount++;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (_Logger != null)
                        {
                            _Logger.LogError("Exception processing data from socket: " + ex.StackTrace);
                            _Logger.LogError("Continuing...");
                        }
                    }
                }
            }
            catch (StackOverflowException ex)
            {
                if (_Logger != null)
                {
                    _Logger.LogError("Stack Overflow while processing data from socket: " + ex.StackTrace);
                    _Logger.LogError("Closing program...");
                }

                throw;
            }
            catch (OutOfMemoryException ex)
            {
                if (_Logger != null)
                {
                    _Logger.LogError("Stack Overflow while processing data from socket: " + ex.StackTrace);
                    _Logger.LogError("Closing program...");
                }

                throw;
            }
            catch (SocketException ex)
            {
                if (_Logger != null)
                {
                    _Logger.LogError("Socket exception processing data from socket: " + ex.StackTrace + ex.Message);
                    _Logger.LogError("Continuing...");
                }

                // Dinar: this will raise every time when sensor stopped connection
                // wont throw to not stop service
                //throw;
            }
        }
    }

}

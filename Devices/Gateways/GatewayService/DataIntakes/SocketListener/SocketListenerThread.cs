
using Microsoft.ConnectTheDots.Gateway.DataIntake;

namespace SocketListener
{
    using System;
    using System.Net.Sockets;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ConnectTheDots.Common;
    using Microsoft.ConnectTheDots.Gateway;

    //--//

    public class SocketListenerThread : DataIntakeAbstract
    {
        const int CONNECTION_RETRIES = 20;
        const int SLEEP_TIME_BETWEEN_RETRIES = 1000; // 1 sec

        private Func<string, int> _Enqueue;
        private bool _DoWorkSwitch;

        private Thread listeningThread = null;
        private SensorEndpoint _Endpoint;

        public SocketListenerThread( ILogger logger )
            : base( logger ) 
        {
        }

        public override bool  Start( Func<string, int> enqueue )
        {
            _Enqueue = enqueue;

            _DoWorkSwitch = true;

            var sh = new SafeAction<int>( ( t ) => RunForSocket( t ), _Logger );

            Task.Run( () => sh.SafeInvoke( CONNECTION_RETRIES ) );

            return true;
        }

        public override bool Stop( )
        {
            _DoWorkSwitch = false;

            return true;
        }

        public override bool SetEndpoint(SensorEndpoint endpoint = null)
        {
            if( endpoint == null )
            {
                //we need to know endpoint
                return false;
            }

            _Endpoint = endpoint;

            return true;
        }

        private int RunForSocket( int retries )
        {
            int step = retries;

            Socket client = null;
            while (_DoWorkSwitch)//--step > 0 &&
            {
                try
                {
                    _Logger.LogInfo("Try connecting to device - step: " + (CONNECTION_RETRIES - step));

                    client = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Unspecified );

                    client.Connect(_Endpoint.Host, _Endpoint.Port);

                    if (client.Connected)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _Logger.LogError("Exception when opening socket:" + ex.StackTrace);
                    _Logger.LogError("Will retry in 1 second");
                }

                // wait and try again
                Thread.Sleep(SLEEP_TIME_BETWEEN_RETRIES);
            }

            if (client != null && client.Connected)
            {
                _Logger.LogInfo(string.Format("Socket connected to {0}", client.RemoteEndPoint.ToString()));

                listeningThread = new Thread(() => SensorDataClient(client));
                listeningThread.Start();

                _Logger.LogInfo(string.Format("Reader thread started"));
                
                listeningThread.Join();
                
                _Logger.LogInfo("Listening thread terminated. Quitting.");
            }
            else
            {
                _Logger.LogError("No sensor connection detected. Quitting.");
            }
            return 0;
        }

        public void SensorDataClient(Socket client)
        {
            try
            {
                StringBuilder jsonBuilder = new StringBuilder();
                byte[] buffer = new Byte[1024];
                // Use Regular Expressions (Regex) to parse incoming data, which may contain multiple JSON strings 
                // USBSPLSOCKET.PY uses "<" and ">" to terminate JSON string at each end, so built Regex to find strings surrounded by angle brackets
                // You can test Regex extractor against a known string using a variety of online tools, such as http://regexhero.net/tester/ for C#.
                //Regex dataExtractor = new Regex(@"<(\d+.?\d*)>");
                Regex dataExtractor = new Regex("<([\\w\\s\\d:\",-{}.]+)>");

                while (_DoWorkSwitch)
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
                        _Logger.LogError("Exception processing data from socket: " + ex.StackTrace);
                        _Logger.LogError("Continuing...");
                    }
                }
            }
            catch (StackOverflowException ex)
            {
                _Logger.LogError("Stack Overflow while processing data from socket: " + ex.StackTrace);
                _Logger.LogError("Closing program...");

                throw;
            }
            catch (OutOfMemoryException ex)
            {
                _Logger.LogError("Stack Overflow while processing data from socket: " + ex.StackTrace);
                _Logger.LogError("Closing program...");

                throw;
            }
            catch (SocketException ex)
            {
                _Logger.LogError("Socket exception processing data from socket: " + ex.StackTrace + ex.Message);
                _Logger.LogError("Continuing...");

                // Dinar: this will raise every time when sensor stopped connection
                // wont throw to not stop service
                //throw;
            }
        }
    }

}

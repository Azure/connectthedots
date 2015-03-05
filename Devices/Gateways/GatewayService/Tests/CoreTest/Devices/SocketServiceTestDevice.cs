namespace Microsoft.ConnectTheDots.Test
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Microsoft.ConnectTheDots.Common;
    using Microsoft.ConnectTheDots.Gateway;

    //--//

    public class SocketServiceTestDevice
    {
        private const int SLEEP_TIME_MS = 1000;

        //--//

        private readonly ILogger _logger;

        //--//

        public SocketServiceTestDevice( ILogger logger )
        {
            if( logger == null )
            {
                throw new ArgumentException( "Please provide logger to SocketServiceDevice" );
            }

            _logger = logger;
        }


        public void Start( SensorEndpoint endpoint )
        {
            var sh = new SafeAction<SensorEndpoint>( e => RunSocketServer( e ), _logger );

            Task.Run( ( ) => sh.SafeInvoke( endpoint ) );
        }

        public void RunSocketServer( SensorEndpoint endpoint )
        {
            IPAddress ipAddress;
            if( !IPAddress.TryParse( endpoint.Host, out ipAddress ) )
                return;

            _logger.LogInfo( "Starting Socket server..." );

            TcpListener serverSocket = new TcpListener( ipAddress, endpoint.Port );
            serverSocket.Start( );

            TcpClient clientSocket = serverSocket.AcceptTcpClient( );
            _logger.LogInfo( "Accepted connection from client." );

            try
            {
                for( ; ; )
                {
                    NetworkStream networkStream = clientSocket.GetStream( );

                    //byte[] bytesFrom = new byte[10025];
                    //networkStream.Read(bytesFrom, 0, clientSocket.ReceiveBufferSize);

                    //string dataFromClient = Encoding.ASCII.GetString(bytesFrom);
                    //dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));

                    SensorDataContract sensorData = RandomSensorDataGenerator.Generate( );
                    string serializedData = JsonConvert.SerializeObject( sensorData );

                    Byte[] sendBytes = Encoding.ASCII.GetBytes( "<" + serializedData + ">" );

                    networkStream.Write( sendBytes, 0, sendBytes.Length );
                    networkStream.Flush( );

                    _logger.LogInfo( "Sent: " + serializedData );
                    Thread.Sleep( SLEEP_TIME_MS );
                }
            }
            catch( Exception ex )
            {
                _logger.LogError( ex.ToString( ) );
            }

            try
            {
                serverSocket.Stop( );
            }
            catch( Exception ex )
            {
                _logger.LogError( ex.ToString( ) );
            }
        }
    }
}

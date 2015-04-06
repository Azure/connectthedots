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


namespace Microsoft.ConnectTheDots.Test
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using Newtonsoft.Json;
    using Microsoft.ConnectTheDots.Common;
    using Microsoft.ConnectTheDots.Gateway;
    using Microsoft.ConnectTheDots.Common.Threading;

    //--//

    public class SocketClientTestDevice
    {
        private const int CONNECTION_RETRIES           = 20000;
        private const int SLEEP_TIME_BETWEEN_RETRIES   = 1000;  // 1 sec
        private const int TIME_BETWEEN_DATA_MS         = 500;   // 0.5 sec
        

        //--//

        private readonly ILogger  _logger;

        //--//

        private static TcpListener _serverSocket;

        //--//

        private Func<string, int>      _enqueue;
        private bool                   _doWorkSwitch;
        private Thread                 _listeningThread;
        private SensorEndpoint         _endpoint;
        private int                    _messagesToSend;

        //--//

        public SocketClientTestDevice( ILogger logger )
        {
            if( logger == null )
            {
                throw new ArgumentException( "Please provide logger to SocketServiceDevice" );
            }

            _logger = logger;
        }

        public void Stop( )
        {
            _doWorkSwitch = false;
        }
        public void Start( SensorEndpoint endpoint, int messagesToSend )
        {
            _messagesToSend = messagesToSend;
            _endpoint = endpoint;
            _doWorkSwitch = true;

            var sh = new SafeAction<int>( e => RunSocketAsClient( e ), _logger );

            TaskWrapper.Run( ( ) => sh.SafeInvoke( CONNECTION_RETRIES ) );
        }

        private int RunSocketAsClient( int retries )
        {
            int step = retries;

            while( --step > 0 && _doWorkSwitch )
            {
                try
                {
                    _logger.LogInfo( "Try connecting to gateway" );

                    Socket client = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Unspecified );

                    client.Connect( _endpoint.Host, _endpoint.Port );

                    if( client.Connected )
                    {

                        _logger.LogInfo( string.Format( "Socket connected to {0}", client.RemoteEndPoint.ToString() ) );

                        _listeningThread = new Thread( ( ) => StartDataFlow( client ) );
                        _listeningThread.Start( );

                        _listeningThread.Join( );

                        //reset number of retries to connect
                        step = retries;
                    }
                }
                catch( Exception ex )
                {
                    _logger.LogError( "Exception when opening socket:" + ex.StackTrace );
                }

                // wait and try again
                Thread.Sleep( SLEEP_TIME_BETWEEN_RETRIES );
            }

            return 0;
        }

        private void StartDataFlow( Socket client )
        {
            for (; _messagesToSend != 0 && _doWorkSwitch; --_messagesToSend)
            {
                try
                {
                    if( !client.Connected )
                    {
                        client.Close( );
                        break;
                    }

                    SensorDataContract sensorData = RandomSensorDataGenerator.Generate( );
                    string serializedData = JsonConvert.SerializeObject( sensorData );

                    Byte[] sendBytes = Encoding.ASCII.GetBytes( "<" + serializedData + ">" );

                    client.Send( sendBytes );

                    Thread.Sleep( TIME_BETWEEN_DATA_MS );
                }
                catch( Exception ex )
                {
                    _logger.LogError( "Exception processing data from socket: " + ex.StackTrace );
                    _logger.LogError( "Continuing..." );
                }
            }
        }
    }
}

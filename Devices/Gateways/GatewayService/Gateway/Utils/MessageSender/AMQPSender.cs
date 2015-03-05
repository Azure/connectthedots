namespace Microsoft.ConnectTheDots.Gateway
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Amqp;
    using Amqp.Framing;
    using Newtonsoft.Json;
    using Microsoft.ConnectTheDots.Common;

    //--//

    public class AMQPSender<T> : IMessageSender<T>
    {

        //--//

        internal class ReliableSender
        {
            private readonly object _syncRoot = new object( );

            //--//

            private readonly string     _eventHubName;
            private          Address    _address;
            private          SenderLink _sender;
            private          bool       _alive;
            private          ILogger    _Logger;

            //--//

            internal ReliableSender( string amqpsAddress, string eventHubName, ILogger logger )
            {
                _eventHubName = eventHubName;
                _Logger = SafeLogger.FromLogger( logger );

                try
                {
                    _address = new Address( amqpsAddress );
                }
                catch( Exception ex )
                {
                    throw new ArgumentException( "Please use a correct amqp address: " + ex.Message );
                }

                EstablishSender( );
            }

            internal SenderLink Sender
            {
                get
                {
                    EstablishSender( );

                    return _sender;
                }
            }

            internal void SetDead( )
            {
                if( _alive )
                {
                    lock( _syncRoot )
                    {
                        if( _alive )
                        {
                            _alive = false;

                            if( _sender != null )
                            {
                                _sender.Closed -= OnSenderClosedCallback;

                                _sender.Close( );
                            }
                        }
                    }
                }
            }

            internal void Close( )
            {
                _sender.Close( STOP_TIMEOUT_MS );
            }

            protected void EstablishSender( )
            {
                try
                {
                    if( _alive == false )
                    {
                        lock( _syncRoot )
                        {
                            if( _alive == false )
                            {
                                try
                                {
                                    Connection connection = new Connection( _address );
                                    Session session = new Session( connection );

                                    _sender = new SenderLink( session, "send-link:" + _eventHubName, _eventHubName );

                                    _sender.Closed += OnSenderClosedCallback;

                                    _alive = true;
                                }
                                catch( Exception ex )
                                {
                                    _Logger.LogError( "Error on lock: " + JsonConvert.SerializeObject( ex ) );
                                }
                            }
                        }
                    }
                }
                catch( Exception ex )
                {
                    //we don't want service to stop working when exception was thrown at connection creation 
                    //TODO: add reraise for some cases
                    _Logger.LogError( "Error on establishing sender: " + JsonConvert.SerializeObject( ex ) );
                }
            }

            protected void OnSenderClosedCallback( AmqpObject sender, Error error )
            {
                _Logger.LogError( "OnSenderClosedCallback: " + error.Info + error.Description );
                // signal the connection will fail 
                SetDead( );

                // re-create the connection pro-actively
                EstablishSender( );
            }
        }

        //--//

        internal class SendersPool
        {
            public static readonly int MAX_POOL_SIZE = 64;

            //--//

            private readonly object _sync = new object( );
            
            //--//

            private readonly ILogger          _logger;
            private readonly ReliableSender[] _pool;
            private          int              _current;

            //--//

            public SendersPool( string amqpAddress, string eventHubName, int size, ILogger logger )
            {
                _logger = logger;

                if( size > MAX_POOL_SIZE )
                {
                    size = MAX_POOL_SIZE;
                }

                _pool = new ReliableSender[ size ];

                for( int i = 0; i < size; ++i )
                {
                    _pool[ i ] = new ReliableSender( amqpAddress, eventHubName, logger );
                }

                _current = 0;
            }

            public ReliableSender PickSender( )
            {
                ReliableSender rs;

                lock( _sync )
                {
                    rs = _pool[ _current ];

                    _current = ( _current + 1 ) % _pool.Length;
                }

                return rs;
            }

            public void Close( )
            {
                lock( _sync )
                {
                    foreach( ReliableSender rs in _pool )
                    {
                        rs.Close( );
                    }
                }
            }
        }

        //--//

        private const           int     STOP_TIMEOUT_MS = 5000; // ms

        //--//

        private static readonly string _logMesagePrefix = "AMQPSender error. ";

        //--//

        private readonly string         _defaultSubject;
        private readonly string         _defaultDeviceId;
        private readonly string         _defaultDeviceDisplayName;
        private          SendersPool    _senders;

        public ILogger Logger { private get; set; }

        public AMQPSender( string amqpsAddress, string eventHubName, string defaultSubject, string defaultDeviceId, string defaultDeviceDisplayName, ILogger logger )
        {
            Logger = SafeLogger.FromLogger( logger );

            Logger.LogInfo( "Connecting to Event hub" );

            if( defaultSubject == null || defaultDeviceId == null || defaultDeviceDisplayName == null || eventHubName == null )
            {
                throw new ArgumentException( "defaultSubject, defaultDeviceId, defaultDeviceDisplayName, eventHubName cannot be null" );
            }

            _defaultSubject = defaultSubject;
            _defaultDeviceId = defaultDeviceId;
            _defaultDeviceDisplayName = defaultDeviceDisplayName;

            _senders = new SendersPool( amqpsAddress, eventHubName, Constants.ConcurrentConnections, Logger );
        }

        public async Task SendMessage( T data )
        {
            try
            {
                if( data == null )
                {
                    return;
                }

                string jsonData = JsonConvert.SerializeObject( data );

                await PrepareAndSend( jsonData );
            }
            catch( Exception ex )
            {
                Logger.LogError( _logMesagePrefix + ex.Message );
            }
        }

        public async Task SendSerialized( string jsonData )
        {
            try
            {
                if( String.IsNullOrEmpty( jsonData ) )
                {
                    return;
                }

                await PrepareAndSend( jsonData );
            }
            catch( Exception ex )
            {
                Logger.LogError( _logMesagePrefix + ex.Message );
            }
        }

        public void Close( )
        {
            Logger.LogInfo( "Close signal to AMQP recieved" );

            _senders.Close( );
        }

        private async Task PrepareAndSend( string jsonData )
        {
            Message msg = PrepareMessage( jsonData );
            // send to the cloud asynchronously, but wait for completetion
            // this is actually serializing access to the SenderLink type

            var sh = new SafeAction<Message>( m => SendAmqpMessage( m ), Logger );

            await Task.Run( ( ) => sh.SafeInvoke( msg ) );
        }

        private void SendAmqpMessage( Message m )
        {
            bool firstTry = true;

            ReliableSender rl = _senders.PickSender( );

            while( true )
            {
                try
                {
                    rl.Sender.Send( m, SendOutcome, rl );
                    break;
                }
                catch( Exception ex )
                {
                    Logger.LogError( "Exception on send" + ex.Message );

                    if( firstTry )
                    {
                        firstTry = false;

                        rl.SetDead( );
                    }
                    else
                    {
                        // re-trhrow the exception if we already re-tried
                        throw;
                    }
                }
            }
        }

        protected Message PrepareMessage( string serializedData, string subject = default(string), string deviceId = default(string), string deviceDisplayName = default(string) )
        {
            if( subject == default( string ) )
                subject = _defaultSubject;

            if( deviceId == default( string ) )
                deviceId = _defaultDeviceId;

            if( deviceDisplayName == default( string ) )
                deviceDisplayName = _defaultDeviceDisplayName;

            var creationTime = DateTime.UtcNow;

            bool setMessageData = false;

            Message message = null;

            //// Event Hub partition key: device id - ensures that all messages from this device go to the same partition and thus preserve order/co-location at processing time
            //message.MessageAnnotations[new Symbol("x-opt-partition-key")] = deviceId;

            Dictionary<string, object> outDictionary = null;
            if( serializedData != null )
            {
                //string serializedData = JsonConvert.SerializeObject( messageData );
                outDictionary =
                    JsonConvert.DeserializeObject<Dictionary<string, object>>( serializedData );

                outDictionary[ "Subject" ] = subject; // Message Type
                outDictionary[ "from" ] = deviceId; // Originating device
                outDictionary[ "dspl" ] = deviceDisplayName; // Display name for originating device

                setMessageData = true;
            }

            if( setMessageData )
            {
                message = new Message( new Data
                {
                    Binary = Encoding.UTF8.GetBytes( JsonConvert.SerializeObject( outDictionary ) )
                } )
                {
                    Properties = new Properties
                    {
                        Subject = subject, // Message type
                        CreationTime = creationTime, // Time of data sampling
                    },
                    MessageAnnotations = new MessageAnnotations( ),
                    ApplicationProperties = new ApplicationProperties( )
                };
                message.Properties.ContentType = "text/json";
            }
            else
            {
                message = new Message
                {
                    Properties = new Properties
                    {
                        Subject = subject, // Message type
                        CreationTime = creationTime, // Time of data sampling
                    },
                    MessageAnnotations = new MessageAnnotations( ),
                    ApplicationProperties = new ApplicationProperties( )
                };
                // No data: send an empty message with message type "weather error" to help diagnose problems "from the cloud"
                message.Properties.Subject = subject + "err";
            }

            return message;
        }

        int _sentMessages = 0;
        DateTime _start;
        private void SendOutcome( Message message, Outcome outcome, object state )
        {
            int sent = Interlocked.Increment( ref _sentMessages );

            string messageToLog = Encoding.UTF8.GetString( message.Encode( ).Buffer );

            int jsonBracketIndex = messageToLog.IndexOf( "{", System.StringComparison.Ordinal );
            if( jsonBracketIndex > 0 )
            {
                messageToLog = messageToLog.Substring( jsonBracketIndex );
            }

            jsonBracketIndex = messageToLog.LastIndexOf( "}", System.StringComparison.Ordinal );
            if( jsonBracketIndex > 0 )
            {
                messageToLog = messageToLog.Substring( 0, jsonBracketIndex + 1 );
            }

            if( outcome is Accepted )
            {
                Logger.LogInfo( "Message is accepted" );

                if( sent == 1 )
                {
                    _start = DateTime.Now;
                }

                if( Interlocked.CompareExchange( ref _sentMessages, 0, Constants.MessagesLoggingThreshold ) ==
                    Constants.MessagesLoggingThreshold )
                {
                    DateTime now = DateTime.Now;

                    TimeSpan elapsed = ( now - _start );

                    _start = now;

                    var sh = new SafeAction<String>( s => Logger.LogInfo( s ), Logger );

                    Task.Run( ( ) => sh.SafeInvoke(
                        String.Format( "GatewayService sent {0} events to Event Hub succesfully in {1} ms ",
                                Constants.MessagesLoggingThreshold, elapsed.TotalMilliseconds.ToString( ) ) ) );
                }
            }
            else
            {
                Logger.LogInfo( "Message is rejected: " + messageToLog );
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amqp;
using Amqp.Framing;
using Gateway.Utils.Logger;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Gateway.Utils.MessageSender
{
    public class AMQPSender<T> : IMessageSender<T>
    {
        private const int STOP_TIMEOUT_MS = 5000; // ms

        internal class ReliableSender
        {
            private readonly string _EventHubName;

            private Address _address;
            private SenderLink _sender;
            private bool _alive;

            private object _sync = new object( );

            internal ReliableSender( string amqpsAddress, string eventHubName )
            {
                _EventHubName = eventHubName;

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

            internal void SetDead()
            {
                if( _alive )
                {
                    lock( _sync)
                    { 
                        if( _alive)
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

            internal void Close()
            {
                _sender.Close( STOP_TIMEOUT_MS );
            }

            protected void EstablishSender()
            {
                if( _alive == false )
                {
                    lock( _sync )
                    {
                        if( _alive == false )
                        {
                            Connection connection = new Connection( _address );
                            Session session = new Session( connection );

                            _sender = new SenderLink( session, "send-link:" + _EventHubName, _EventHubName );

                            _sender.Closed += OnSenderClosedCallback;

                            _alive = true;
                        }
                    }
                }
            }

            protected void OnSenderClosedCallback( AmqpObject sender, Error error )
            {
                // signal the connection will fail 
                SetDead( );

                // re-create the connection pro-actively
                EstablishSender( );
            }
        }

        internal class SendersPool
        {
            public static readonly int MAX_POOL_SIZE = 64;

            private ReliableSender[] _pool;
            private int _current;
            private readonly object _sync = new object();

            public SendersPool( string amqpAddress, string eventHubName, int size )
            {
                if(size > MAX_POOL_SIZE)
                {
                    size = MAX_POOL_SIZE;
                }

                _pool = new ReliableSender[ size ];

                for(int i = 0; i < size; ++i)
                {
                    _pool[i] = new ReliableSender(amqpAddress, eventHubName); 
                }

                _current = 0;
            }

            public ReliableSender PickSender()
            {
                ReliableSender rs;

                lock( _sync )
                {
                    rs = _pool[_current]; 
                    
                    _current = (_current + 1) % _pool.Length;
                }

                return rs;
            }

            public void Close()
            {
                lock( _sync )
                {
                    foreach (ReliableSender rs in _pool)
                    {
                        rs.Close();
                    }
                }
            }
        }


        private readonly string _DefaultSubject;
        private readonly string _DefaultDeviceId;
        private readonly string _DefaultDeviceDisplayName;

        private SendersPool _senders;

        public ILogger Logger { private get; set; }

        private string _LogMesagePrefix = "AMQPSender error. ";

        public string LogMessagePrefix
        {
            set { _LogMesagePrefix = value; }
        }

        public AMQPSender(string amqpsAddress, string eventHubName, string defaultSubject, string defaultDeviceId, string defaultDeviceDisplayName)
        {
            if (defaultSubject == null || defaultDeviceId == null || defaultDeviceDisplayName == null || eventHubName == null)
            {
                throw new ArgumentException("defaultSubject, defaultDeviceId, defaultDeviceDisplayName, eventHubName cannot be null");
            }

            _DefaultSubject = defaultSubject;
            _DefaultDeviceId = defaultDeviceId;
            _DefaultDeviceDisplayName = defaultDeviceDisplayName;

            _senders = new SendersPool(amqpsAddress, eventHubName, Constants.ConcurrentConnections);
        }

        public async Task SendMessage(T data)
        {
            var m = PrepareMessage( data );

            try
            {
                // send to the cloud asynchronously, but wait for completetion
                // this is actually serializing access to the SenderLink type
                await Task.Run(() => SendAmqpMessage( m ));
            }
            catch (Exception ex)
            {
                Logger.LogError(_LogMesagePrefix + ex.Message);
            }
        }

        public void Close()
        {
            _senders.Close( );
        }

        private void SendAmqpMessage( Message m )
        {
            bool firstTry = true;

            ReliableSender rl = _senders.PickSender();

            while( true )
            {
                try
                {
                    rl.Sender.Send( m, SendOutcome, rl );

                    break;
                }
                catch(Exception )
                {
                    if( firstTry )
                    {
                        firstTry = false;

                        rl.SetDead();
                    }
                    else
                    {
                        // re-trhrow the exception if we already re-tried
                        throw;
                    }
                }
            }
        }

        protected Message PrepareMessage( T messageData, string subject = default(string), string deviceId = default(string), string deviceDisplayName = default(string) )
        {
            if( subject == default( string ) )
                subject = _DefaultSubject;

            if( deviceId == default( string ) )
                deviceId = _DefaultDeviceId;

            if( deviceDisplayName == default( string ) )
                deviceDisplayName = _DefaultDeviceDisplayName;

            var creationTime = DateTime.UtcNow;

            bool setMessageData = false;

            Message message = null;
            
            //// Event Hub partition key: device id - ensures that all messages from this device go to the same partition and thus preserve order/co-location at processing time
            //message.MessageAnnotations[new Symbol("x-opt-partition-key")] = deviceId;

            Dictionary<string, object> outDictionary = null;
            if( messageData != null )
            {
                string serializedData = JsonConvert.SerializeObject( messageData );
                outDictionary =
                    JsonConvert.DeserializeObject<Dictionary<string, object>>( serializedData );

                outDictionary["Subject"] = subject; // Message Type
                outDictionary[ "time" ] = creationTime;
                outDictionary[ "from" ] = deviceId; // Originating device
                outDictionary[ "dspl" ] = deviceDisplayName; // Display name for originating device

                setMessageData = true;
            }

            if(setMessageData)
            {
                message = new Message(new Data
                {
                    Binary = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(outDictionary))
                })
                {
                    Properties = new Properties
                    {
                        Subject = subject, // Message type
                        CreationTime = creationTime, // Time of data sampling
                    },
                    MessageAnnotations = new MessageAnnotations(),
                    ApplicationProperties = new ApplicationProperties()
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
                    MessageAnnotations = new MessageAnnotations(),
                    ApplicationProperties = new ApplicationProperties()
                };
                // No data: send an empty message with message type "weather error" to help diagnose problems "from the cloud"
                message.Properties.Subject = subject + "err";
            }

            message.ApplicationProperties["time"] = creationTime;
            message.ApplicationProperties["from"] = deviceId; // Originating device
            message.ApplicationProperties["dspl"] = deviceDisplayName; // Display name for originating device


            return message;
        }

        int _sentMessages = 0;
        DateTime _start;
        private void SendOutcome(Message message, Outcome outcome, object state)
        {
            int sent = Interlocked.Increment(ref _sentMessages);

            if( outcome is Accepted ) 
            {
                if(sent == 1)
                {
                    _start = DateTime.Now;
                }
                
                if (Interlocked.CompareExchange( ref _sentMessages, 0, Constants.MessagesLoggingThreshold) == Constants.MessagesLoggingThreshold)
                {
                    DateTime now = DateTime.Now;

                    TimeSpan elapsed = (now - _start);

                    _start = now;

                    Task.Run( () => 
                        {
                            Logger.LogInfo( 
                                String.Format( "GatewayService sent {0} events to Event Hub succesfully in {1} ms ", Constants.MessagesLoggingThreshold, elapsed.TotalMilliseconds.ToString( ) ) 
                                );
                        });
                }
            }
        }
    }
}

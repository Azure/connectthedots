//#define MOCK_SENDER

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoreTest.Utils.MessageSender;
using Gateway;
using Gateway.DataIntake;
using Gateway.Models;
using Gateway.Utils.Loader;
using Gateway.Utils.MessageSender;
using Gateway.Utils.Queue;
using CoreTest.Utils.Logger;
using CoreTest.Utils.Loader;
using SharedInterfaces;
using System.Text;

namespace CoreTest
{
    public class CoreTest : ITest
    {
        public const int TEST_ITERATIONS = 5;
        public const int MAX_TEST_MESSAGES = 1000;

        private readonly ILogger _testLogger = new TestLogger();
        private readonly AutoResetEvent _completed = new AutoResetEvent(false);
        private readonly GatewayQueue<QueuedItem> _GatewayQueue;
        private readonly IMessageSender<SensorDataContract> _Sender;
        private readonly BatchSenderThread<QueuedItem, SensorDataContract> _BatchSenderThread;
        private readonly Random _rand;
        private int _totalMessagesSent;
        private int _totalMessagesToSend;

        private const int STOP_TIMEOUT_MS = 5000; // ms

        public CoreTest()
        {
            _rand = new Random( );
            _totalMessagesSent = 0;
            _totalMessagesToSend = 0;
            _GatewayQueue = new GatewayQueue<QueuedItem>( );
#if MOCK_SENDER
            _Sender = new MockSender<SensorDataContract>(this);
#else

            AMQPConfig amqpConfig = Loader.GetAMQPConfig( );
                
            _Sender = new AMQPSender<SensorDataContract>(
                                                amqpConfig.AMQPSAddress,
                                                amqpConfig.EventHubName,
                                                amqpConfig.EventHubMessageSubject,
                                                amqpConfig.EventHubDeviceId,
                                                amqpConfig.EventHubDeviceDisplayName,
                                                _testLogger
                                                );
#endif

            _BatchSenderThread = new BatchSenderThread<QueuedItem, SensorDataContract>( 
                _GatewayQueue, 
                _Sender,
                m => DataTransforms.AddTimeCreated(DataTransforms.SensorDataContractFromQueuedItem(m, _testLogger)), 
                null );
        }

        public void Run()
        {
            TestRepeatSend( );
            TestDataIntake();
        }

        public void TestRepeatSend()
        {
            try
            {
                GatewayService service = PrepareGatewayService();

                // Send a flurry of messages, repeat a few times

                // script message sequence
                int[] sequence = new int[TEST_ITERATIONS];
                for (int iteration = 0; iteration < TEST_ITERATIONS; ++iteration)
                {
                    int count = _rand.Next(MAX_TEST_MESSAGES);

                    sequence[iteration] = count;

                    _totalMessagesToSend += count;
                }

                const float mean = 39.6001f;
                const int range  = 10;

                Random rand = new Random( (int)(DateTime.Now.Ticks >> 32) ); 

                // send the messages
                for (int iteration = 0; iteration < TEST_ITERATIONS; ++iteration)
                {
                    int count = sequence[iteration];

                    while (--count >= 0)
                    {
                        //
                        // Build a message. 
                        // It will look something like this: 
                        // "{\"unitofmeasure\":\"%\",\"location\":\"Olivier's office\",\"measurename\":\"Humidity\",\"timecreated\":\"2/26/2015 12:50:29 AM\",\"organization\":\"MSOpenTech\",\"guid\":\"00000000-0000-0000-0000-000000000000\",\"value\":39.600000000000001,\"displayname\":\"NETMF\"}"
                        // 

                        bool add = (rand.Next( ) % 2) == 0;
                        int variant = rand.Next( ) % range;
                        float value = mean; 

                        StringBuilder sb = new StringBuilder( );
                        sb.Append( "{\"unitofmeasure\":\"%\",\"location\":\"Olivier's office\",\"measurename\":\"Humidity\"," );
                        sb.Append( "\"timecreated\":\"" );
                        sb.Append( DateTime.UtcNow.ToString( ) ); // this should look like "2015-02-25T23:07:47.159Z"
                        sb.Append( "\",\"organization\":\"MSOpenTech\",\"guid\":\"" );
                        sb.Append( new Guid().ToString() );
                        sb.Append( "\",\"value\":" );
                        sb.Append( (value += add ? variant : -variant).ToString() );
                        sb.Append( ",\"displayname\":\"NETMF\"}" );

                        string message = sb.ToString();

                        service.Enqueue( message ); 

                        DataArrived( message ); 
                    }
                }

                Debug.Assert( _totalMessagesSent == _totalMessagesToSend );

                _completed.WaitOne();

                _BatchSenderThread.Stop( STOP_TIMEOUT_MS ); 
            }
            catch (Exception ex)
            {
                _testLogger.LogError("exception caught: " + ex.StackTrace);
            }
            finally
            {
                _BatchSenderThread.Stop(STOP_TIMEOUT_MS);
                _Sender.Close( );
            }
        }

        public void TestDataIntake()
        {
            try
            {
                GatewayService service = PrepareGatewayService();

                DataIntakeLoader dataIntakeLoader = new DataIntakeLoader( Loader.GetSources( ), Loader.GetEndpoints(), _testLogger ); 

                _totalMessagesToSend += 5;

                dataIntakeLoader.StartAll( service.Enqueue, DataArrived ); 

                _completed.WaitOne();

                dataIntakeLoader.StopAll( );

                _BatchSenderThread.Stop(STOP_TIMEOUT_MS);
            }
            catch (Exception ex)
            {
                _testLogger.LogError("exception caught: " + ex.StackTrace);
            }
            finally
            {
                _BatchSenderThread.Stop(STOP_TIMEOUT_MS);
                _Sender.Close();
            }
        }

        public int TotalMessagesSent
        {
            get
            {
                return _totalMessagesSent;
            }
        }

        public int TotalMessagesToSend
        {
            get
            {
                return _totalMessagesToSend;
            }
        }

        public void Completed()
        {
            _completed.Set( );

            Console.WriteLine( String.Format( "Test completed, {0} messages sent", _totalMessagesToSend ) );
        }

        private GatewayService PrepareGatewayService()
        {
            _BatchSenderThread.Logger = _testLogger;
            _BatchSenderThread.Start();

            GatewayService service = new GatewayService(_GatewayQueue, _BatchSenderThread);

            service.Logger = _testLogger;
            service.OnDataInQueue += DataInQueue;

            _BatchSenderThread.OnEventsBatchProcessed += EventBatchProcessed;

            return service;
        }

        protected void DataArrived( string data )
        {
            _totalMessagesSent++;
        }

        protected virtual void DataInQueue( QueuedItem data )
        {
            // LORENZO: test behaviours such as accumulating data an processing in batch
            // as it stands, we are processing every event as it comes in

            _BatchSenderThread.Process( );
        }

        protected virtual void EventBatchProcessed( List<Task> messages )
        {
            // LORENZO: test behaviours such as waiting for messages to be delivered or re-transmission
            
            foreach(Task t in messages)
            {
                _testLogger.LogInfo(String.Format("Task {0} status is '{1}'", t.Id, t.Status.ToString()));
            }

            Task.WaitAll(((List<Task>)messages).ToArray());

            foreach(Task t in messages)
            {
                _testLogger.LogInfo(String.Format("Task {0} status is '{1}'", t.Id, t.Status.ToString()));
            }
        }
    }
}


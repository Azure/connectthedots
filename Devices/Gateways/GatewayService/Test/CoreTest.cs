using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Gateway;
using Gateway.Models;
using Gateway.Utils.Logger;
using Gateway.Utils.MessageSender;
using Gateway.Utils.Queue;
using Test.Utils.Logger;

namespace Test
{
    public class CoreTest : ITest
    {
        public const int TEST_ITERATIONS = 100;
        public const int MAX_TEST_MESSAGES = 1000;

        private readonly ILogger _testLogger = new TestLogger();
        private readonly AutoResetEvent _completed = new AutoResetEvent(false);
        private readonly GatewayQueue<SensorDataContract> _GatewayQueue;
        private readonly IMessageSender<SensorDataContract> _Sender;
        private readonly BatchSenderThread<SensorDataContract> _BatchSenderThread;
        private readonly Random _rand;
        private int _totalMessages;

        private const int STOP_TIMEOUT_MS = 5000; // ms

        public CoreTest()
        {
            _rand = new Random();
            _totalMessages = 0;
            _GatewayQueue = new GatewayQueue<SensorDataContract>();
            _Sender = new MockSender<SensorDataContract>(this);
            //_Sender = new AMQPSender<SensorDataContract>(Constants.AMQPSAddress, Constants.EventHubName, Constants.EventHubMessageSubject, Constants.EventHubDeviceId, Constants.EventHubDeviceDisplayName);
            ((AMQPSender<SensorDataContract>)_Sender).Logger = new TestLogger();
            _BatchSenderThread = new BatchSenderThread<SensorDataContract>( _GatewayQueue, _Sender );
        }

        public void Run()
        {
            try
            {
                _BatchSenderThread.Logger = _testLogger;
                _BatchSenderThread.Start();

                GatewayService service = new GatewayService(_GatewayQueue, _BatchSenderThread);

                service.Logger = _testLogger;
                
                service.OnDataInQueue += OnData;
                
                _BatchSenderThread.OnEventsBatchProcessed += OnEventBatchProcessed;

                // Send a flurry of messages, repeat a few times
                for (int iteration = 0; iteration < TEST_ITERATIONS; ++iteration)
                {
                    int count = _rand.Next(MAX_TEST_MESSAGES);

                    _totalMessages += count;

                    while (--count >= 0)
                    {
                        service.Enqueue("42");
                    }
                }

                // LORENZO: check all tasks
                _completed.WaitOne();

                _BatchSenderThread.Stop(STOP_TIMEOUT_MS);
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

        public int TotalMessagesSent
        {
            get
            {
                return _totalMessages;
            }
        }

        public void Completed()
        {
            _completed.Set();
        }

        protected virtual void OnData(SensorDataContract data)
        {
            // LORENZO: test behaviours such as accumulating data an processing in batch
            // as it stands, we are processing every event as it comes in
            _BatchSenderThread.Process();
        }

        protected virtual void OnEventBatchProcessed( List<Task> messages )
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


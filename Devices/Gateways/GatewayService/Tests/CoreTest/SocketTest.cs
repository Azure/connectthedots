using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreTest.Devices;
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

namespace CoreTest
{
    public class SocketTest : ITest
    {
        public const int TEST_ITERATIONS = 5;
        public const int MAX_TEST_MESSAGES = 1000;

        private readonly ILogger _testLogger = new TestLogger();
        private readonly AutoResetEvent _completed = new AutoResetEvent(false);
        private readonly GatewayQueue<QueuedItem> _GatewayQueue;
        private readonly IMessageSender<QueuedItem> _Sender;
        private readonly BatchSenderThread<QueuedItem, QueuedItem> _BatchSenderThread;
        private int _totalMessagesSent;
        private int _totalMessagesToSend;

        private const int STOP_TIMEOUT_MS = 5000; // ms

        public SocketTest()
        {
            _totalMessagesSent = 0;
            _totalMessagesToSend = 0;
            _GatewayQueue = new GatewayQueue<QueuedItem>();
            _Sender = new MockSender<QueuedItem>(this);
            //_Sender = new AMQPSender<SensorDataContract>(Constants.AMQPSAddress, Constants.EventHubName, Constants.EventHubMessageSubject, Constants.EventHubDeviceId, Constants.EventHubDeviceDisplayName);
            //((AMQPSender<QueuedItem>)_Sender).Logger = new TestLogger();
            _BatchSenderThread = new BatchSenderThread<QueuedItem, QueuedItem>(_GatewayQueue, _Sender, m => m, null);
        }

        public void Run()
        {
            TestRecieveMessagesFromSocketDevice();
        }

        public void TestRecieveMessagesFromSocketDevice()
        {
            try
            {
                SocketServiceTestDevice device = new SocketServiceTestDevice(_testLogger);
                SensorEndpoint endpoint = new SensorEndpoint
                {
                    Host = "127.0.0.1",
                    Port = 5000
                };
                device.Start(endpoint);

                GatewayService service = PrepareGatewayService();

                IList<string> sources = Loader.GetSources();
                DataIntakeLoader dataIntakeLoader = new DataIntakeLoader(sources.Where(m => m.Contains("Socket")).ToList(), _testLogger);

                _totalMessagesToSend += 5;

                dataIntakeLoader.StartAll(service.Enqueue, DataArrived);

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
            _completed.Set();

            Console.WriteLine(String.Format("Test completed, {0} messages sent", _totalMessagesToSend));
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

        protected void DataArrived(string data)
        {
            _totalMessagesSent++;
        }

        protected virtual void DataInQueue(QueuedItem data)
        {
            // LORENZO: test behaviours such as accumulating data an processing in batch
            // as it stands, we are processing every event as it comes in

            _BatchSenderThread.Process();
        }

        protected virtual void EventBatchProcessed(List<Task> messages)
        {
            // LORENZO: test behaviours such as waiting for messages to be delivered or re-transmission

            foreach (Task t in messages)
            {
                _testLogger.LogInfo(String.Format("Task {0} status is '{1}'", t.Id, t.Status.ToString()));
            }

            Task.WaitAll(((List<Task>)messages).ToArray());

            foreach (Task t in messages)
            {
                _testLogger.LogInfo(String.Format("Task {0} status is '{1}'", t.Id, t.Status.ToString()));
            }
        }
    }
}


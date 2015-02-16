using System;
using System.Threading;
using BatchSenderThreadTest.Utils.MessageSender;
using Gateway;
using Gateway.Utils.Queue;
using CoreTest.Utils.Logger;

namespace BatchSenderThreadTest
{
    public class BatchSenderThreadTest
    {
        private readonly TestLogger _testLogger = new TestLogger();
        readonly Random _Random = new Random((int)DateTime.Now.Ticks);

        private void TestMessagesGoFromSourceToTarget()
        {
            MockSenderMap<int> targetMap = new MockSenderMap<int>(),
                sourceMap = new MockSenderMap<int>();

            GatewayQueue<int> queue = new GatewayQueue<int>();
            EventProcessor batchSenderThread = new BatchSenderThread<int, int>(queue, targetMap, m => m, null);
            batchSenderThread.Logger = _testLogger;
            batchSenderThread.Start();

            const int batchesIterations = 100;
            const int maxQueuedItemCount = 20;
            const int waitForBatchThreadTimeMs = 22;

            for (int iteration = 0; iteration < batchesIterations; ++iteration)
            {
                int queuedItemCount = _Random.Next(1, maxQueuedItemCount);
                for (int count = 0; count < queuedItemCount; ++count)
                {
                    int itemToQueue = _Random.Next();
                    
                    queue.Push(itemToQueue);

                    sourceMap.SendMessage(itemToQueue).Wait();
                }
                batchSenderThread.Process();
                
                Thread.Sleep(waitForBatchThreadTimeMs);

                if (!targetMap.ContainsOthersItems(sourceMap)
                    || !sourceMap.ContainsOthersItems(targetMap))
                {
                    _testLogger.LogError("Not processed message found");
                    break;
                }
            }

            batchSenderThread.Stop(waitForBatchThreadTimeMs);
        }

        private void TestMessagesGoFromSourceToTargetWithTwoBatchSenderThreads()
        {
            MockSenderAsyncQueue<int> targetQueue = new MockSenderAsyncQueue<int>();
            MockSenderMap<int> sourceMap = new MockSenderMap<int>();

            GatewayQueue<int> queue = new GatewayQueue<int>();
            EventProcessor batchSenderThreadA = new BatchSenderThread<int, int>(queue, targetQueue, m => m, null),
                batchSenderThreadB = new BatchSenderThread<int, int>(queue, targetQueue, m => m, null);

            batchSenderThreadA.Logger = _testLogger;
            batchSenderThreadB.Logger = _testLogger;

            batchSenderThreadA.Start();
            batchSenderThreadB.Start();

            const int waitForBatchThreadTimeMs = 800;
            const int queuedItemCount = 400;

            for (int count = 0; count < queuedItemCount; ++count)
            {
                int itemToQueue = _Random.Next();

                queue.Push(itemToQueue);

                sourceMap.SendMessage(itemToQueue).Wait();
            }
            batchSenderThreadA.Process();
            batchSenderThreadB.Process();

            Thread.Sleep(waitForBatchThreadTimeMs);

            MockSenderMap<int> targetMap = targetQueue.ToMockSenderMap();
            if (!targetMap.ContainsOthersItems(sourceMap)
                || !sourceMap.ContainsOthersItems(targetMap))
            {
                _testLogger.LogError("Not processed message found");
            }

            batchSenderThreadA.Stop(waitForBatchThreadTimeMs);
            batchSenderThreadB.Stop(waitForBatchThreadTimeMs);
        }

        public void Run()
        {
            TestMessagesGoFromSourceToTarget();
            TestMessagesGoFromSourceToTargetWithTwoBatchSenderThreads();
        }
    }
}

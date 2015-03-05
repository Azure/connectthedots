namespace Microsoft.ConnectTheDots.Test
{
    using System;
    using System.Threading;
    using Microsoft.ConnectTheDots.Gateway;
    using Microsoft.ConnectTheDots.Common;

    //--//

    public class BatchSenderThreadTest
    {
        private readonly ILogger _logger;
        private readonly Random  _random;

        //-//

        public BatchSenderThreadTest( ILogger logger )
        {
            if( logger == null )
            {
                throw new ArgumentException( "Cannot run tests without logging" );
            }
            
            _random = new Random( ( int )DateTime.Now.Ticks );

            _logger = logger;
        }

        public void Run( )
        {
            TestMessagesGoFromSourceToTarget( );
            TestMessagesGoFromSourceToTargetWithTwoBatchSenderThreads( );
        }
        private void TestMessagesGoFromSourceToTarget( )
        {
            MockSenderMap<int> targetMap = new MockSenderMap<int>( );
            
            MockSenderMap<int> sourceMap = new MockSenderMap<int>( );

            GatewayQueue<int> queue = new GatewayQueue<int>( );
            
            EventProcessor batchSenderThread = new BatchSenderThread<int, int>( queue, targetMap, m => m, null, _logger );
            
            batchSenderThread.Start( );

            const int batchesIterations = 100;
            const int maxQueuedItemCount = 20;
            const int waitForBatchThreadTimeMs = 22;

            for( int iteration = 0; iteration < batchesIterations; ++iteration )
            {
                int queuedItemCount = _random.Next( 1, maxQueuedItemCount );
                for( int count = 0; count < queuedItemCount; ++count )
                {
                    int itemToQueue = _random.Next( );

                    queue.Push( itemToQueue );

                    sourceMap.SendMessage( itemToQueue ).Wait( );
                }
                batchSenderThread.Process( );

                Thread.Sleep( waitForBatchThreadTimeMs );

                if( !targetMap.ContainsOthersItems( sourceMap )
                    || !sourceMap.ContainsOthersItems( targetMap ) )
                {
                    _logger.LogError( "Not processed message found" );
                    break;
                }
            }

            batchSenderThread.Stop( waitForBatchThreadTimeMs );
        }

        private void TestMessagesGoFromSourceToTargetWithTwoBatchSenderThreads( )
        {
            MockSenderAsyncQueue<int> targetQueue = new MockSenderAsyncQueue<int>( );
            MockSenderMap<int> sourceMap = new MockSenderMap<int>( );

            GatewayQueue<int> queue = new GatewayQueue<int>( );
            EventProcessor batchSenderThreadA = new BatchSenderThread<int, int>( queue, targetQueue, m => m, null, _logger );
            EventProcessor batchSenderThreadB = new BatchSenderThread<int, int>( queue, targetQueue, m => m, null, _logger );

            batchSenderThreadA.Start( );
            batchSenderThreadB.Start( );

            const int waitForBatchThreadTimeMs = 800;
            const int queuedItemCount = 400;

            for( int count = 0; count < queuedItemCount; ++count )
            {
                int itemToQueue = _random.Next( );

                queue.Push( itemToQueue );

                sourceMap.SendMessage( itemToQueue ).Wait( );
            }
            batchSenderThreadA.Process( );
            batchSenderThreadB.Process( );

            Thread.Sleep( waitForBatchThreadTimeMs );

            MockSenderMap<int> targetMap = targetQueue.ToMockSenderMap( );
            if( !targetMap.ContainsOthersItems( sourceMap )
                || !sourceMap.ContainsOthersItems( targetMap ) )
            {
                _logger.LogError( "Not processed message found" );
            }

            batchSenderThreadA.Stop( waitForBatchThreadTimeMs );
            batchSenderThreadB.Stop( waitForBatchThreadTimeMs );
        }        
    }
}

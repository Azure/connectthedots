namespace Microsoft.ConnectTheDots.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ConnectTheDots.Gateway;

    //--//

    internal class MockSenderAsyncQueue<T> : IMessageSender<T>
    {
        protected GatewayQueue<T> _SentMessagesQueue = new GatewayQueue<T>( );

        public async Task SendMessage( T data )
        {
            _SentMessagesQueue.Push( data );
        }
        public Task SendSerialized( string jsonData )
        {
            throw new Exception( "Not implemented" );
        }

        public MockSenderMap<T> ToMockSenderMap( )
        {
            MockSenderMap<T> result = new MockSenderMap<T>( );

            int count = _SentMessagesQueue.Count;
            var tasks = new Task[ count ];
            for( int processedCount = 0; processedCount < count; )
            {
                var popped = _SentMessagesQueue.TryPop( ).Result;
                if( popped.IsSuccess )
                {
                    result.SendMessage( popped.Result ).Wait( );
                    ++processedCount;
                }
            }

            return result;
        }

        public void Close( )
        {
        }
    }
}


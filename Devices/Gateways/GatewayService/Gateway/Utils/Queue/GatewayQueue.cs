namespace Microsoft.ConnectTheDots.Gateway
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Microsoft.ConnectTheDots.Common;

    //--//

    public class GatewayQueue<T> : IAsyncQueue<T>
    {
        private readonly ConcurrentQueue<T> _Queue = new ConcurrentQueue<T>( );

        //--//

        public void Push( T item )
        {
            _Queue.Enqueue( item );
        }

        public Task<OperationStatus<T>> TryPop( )
        {
            Func<OperationStatus<T>> deque = ( ) =>
            {
                T returnedItem;

                bool isReturned = _Queue.TryDequeue( out returnedItem );

                if( isReturned )
                {
                    return OperationStatusFactory.CreateSuccess<T>( returnedItem );
                }

                return OperationStatusFactory.CreateError<T>( ErrorCode.NoDataReceived );
            };

            var sf = new SafeFunc<OperationStatus<T>>( deque, null );

            return Task.Run( ( ) => sf.SafeInvoke( ) );
        }

        public int Count
        {
            get
            {
                return _Queue.Count;
            }
        }
    }
}

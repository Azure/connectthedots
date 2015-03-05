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

        public async Task<OperationStatus<T>> TryPop( )
        {
            try
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

                OperationStatus<T> result = await Task.Run( ( ) => sf.SafeInvoke( ) );

                return result;
            }
            catch( Exception )
            {
                //TODO: Dinar will add logger, or even delete ex handling
                return OperationStatusFactory.CreateError<T>( ErrorCode.NoDataReceived );
            }
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

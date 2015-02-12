using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Gateway.Utils.OperationStatus;

namespace Gateway.Utils.Queue
{
    public class GatewayQueue<T> : IAsyncQueue<T>
    {
        private readonly ConcurrentQueue<T> _Queue = new ConcurrentQueue<T>();

        public void Push(T item)
        {
            _Queue.Enqueue(item);
        }

        public async Task<OperationStatus<T>> TryPop()
        {
            OperationStatus<T> result = await Task.Factory.StartNew(() =>
            {
                T returnedItem;
                bool isReturned = _Queue.TryDequeue(out returnedItem);
                if (isReturned)
                    return OperationStatusFactory.CreateSuccess<T>(returnedItem);
                return OperationStatusFactory.CreateError<T>(ErrorCode.NoDataReceived);
            },
            CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default );

            return result;
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

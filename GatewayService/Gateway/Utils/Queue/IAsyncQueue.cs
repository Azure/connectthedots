using System.Threading.Tasks;
using Gateway.Utils.OperationStatus;

namespace Gateway.Utils.Queue
{
    public interface IAsyncQueue<T>
    {
        void Push(T item);

        Task<OperationStatus<T>> TryPop();

        int Count { get; }
    }
}

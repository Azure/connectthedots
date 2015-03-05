namespace Microsoft.ConnectTheDots.Gateway
{
    using System.Threading.Tasks;

    //--//

    public interface IAsyncQueue<T>
    {
        void Push( T item );

        Task<OperationStatus<T>> TryPop( );

        int Count { get; }
    }
}

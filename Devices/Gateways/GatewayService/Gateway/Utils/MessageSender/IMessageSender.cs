namespace Microsoft.ConnectTheDots.Gateway
{
    using System.Threading.Tasks;

    //--//

    public interface IMessageSender<in T>
    {
        Task SendMessage(T data);
        Task SendSerialized(string jsonData);

        void Close();
    }
}

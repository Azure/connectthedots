using System.Threading.Tasks;

namespace Gateway.Utils.MessageSender
{
    public interface IMessageSender<in T>
    {
        Task SendMessage(T data);
        Task SendSerialized(string jsonData);

        void Close();
    }
}

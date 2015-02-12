using System.Threading.Tasks;

namespace Gateway.Utils.MessageSender
{
    public interface IMessageSender<in T>
    {
        Task SendMessage(T data);

        void Close();
    }
}

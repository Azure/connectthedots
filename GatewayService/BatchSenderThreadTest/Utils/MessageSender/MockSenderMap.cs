using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gateway.Utils.MessageSender;

namespace BatchSenderThreadTest.Utils.MessageSender
{
    internal class MockSenderMap<T> : IMessageSender<T>
    {
        protected SortedDictionary<T, int> _SentMessages = new SortedDictionary<T,int>();

        public async Task SendMessage(T data)
        {
            await Task.Run(() =>
            {
                lock (_SentMessages)
                {
                    if (_SentMessages.ContainsKey(data))
                        _SentMessages[data]++;
                    else _SentMessages.Add(data, 1);
                }
            });
        }

        public bool Contains(T data)
        {
            lock (_SentMessages)
            {
                return _SentMessages.ContainsKey(data);
            }
        }

        public bool ContainsOthersItems(MockSenderMap<T> other)
        {
            lock (_SentMessages)
            {
                if (other._SentMessages.Any(key => !_SentMessages.Contains(key)))
                {
                    return false;
                }
                return true;
            }
        }

        public void Close()
        {
        }
    }
}


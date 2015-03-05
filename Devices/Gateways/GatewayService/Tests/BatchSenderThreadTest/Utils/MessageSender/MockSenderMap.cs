namespace Microsoft.ConnectTheDots.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ConnectTheDots.Common;
    using Microsoft.ConnectTheDots.Gateway;

    //--//

    internal class MockSenderMap<T> : IMessageSender<T>
    {
        protected SortedDictionary<T, int> _SentMessages = new SortedDictionary<T,int>();

        public async Task SendMessage(T data)
        {
            Action<T> send = (d) => {
                lock (_SentMessages)
                {
                    if (_SentMessages.ContainsKey(d))
                        _SentMessages[d]++;
                    else _SentMessages.Add(d, 1);
                }
            };

            var sh = new SafeAction<T>( ( d ) => send( d ), null ); 

            await Task.Run(() => sh.SafeInvoke( data ) ); 
        }

        public Task SendSerialized(string jsonData)
        {
            throw new Exception("Not implemented");
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


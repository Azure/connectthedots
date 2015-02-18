using System.Threading;
using System.Threading.Tasks;
using Gateway.Utils.MessageSender;
using System;

namespace CoreTest.Utils.MessageSender
{
    internal class MockSender<T> : IMessageSender<T>
    {
        private static readonly int MAX_LAG = 50; // ms

        protected readonly ITest _test;
        protected readonly Random _rand;
        protected int _received = 0;

        internal MockSender(ITest test)
        {
            _test = test;
            _rand = new Random();
        }

        public Task SendMessage(T data)
        {
            // Naive atetmpt to simulate network latency
            Thread.Sleep(_rand.Next(MAX_LAG));

            int totalMessages = _test.TotalMessagesSent;

            // LORENZO: print all data and validate that they match the data sent
            if( Interlocked.Increment( ref _received ) == totalMessages && totalMessages >= _test.TotalMessagesToSend)
            {
                _test.Completed();
            }

            return null;
        }

        public Task SendSerialized(string jsonData)
        {
            throw new Exception("Not implemented");
        }

        public void Close()
        {
        }
    }
}


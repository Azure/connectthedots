using System;
using System.Threading;
using System.Threading.Tasks;
using Gateway.Models;
using Gateway.Utils.Logger;
using Gateway.Utils.Queue;

namespace Gateway
{
    public class GatewayService : IGatewayService
    {
        public delegate void DataInQueueEventHandler(SensorDataContract data);

        private readonly IAsyncQueue<SensorDataContract> _Queue;
        private readonly EventProcessor _EventProcessor;

        public GatewayService(IAsyncQueue<SensorDataContract> queue, EventProcessor processor)
        {
            if(queue == null || processor == null)
            {
                throw new ArgumentException("Task queue and event processor cannot be null");
            }

            _Queue = queue;
            _EventProcessor = processor;
        }

        public ILogger Logger { get; set; }

        Random r = new Random();
        private int nall = 0;
        public int Enqueue(string jsonData)
        {
            //Dinar: temporary solution while json is sending deserialized
            SensorDataContract sensorData = new SensorDataContract
                {
                    JsonData = jsonData,
                    DataIntegrity = r.Next(),
                    DataSourceName = jsonData + "Sensor" + r.Next()%3,
                    TagId = nall++,
                    Value = r.Next()%1000-500
                };

            //TODO:we can check status of BatchSender and indicate error on request if needed
            _Queue.Push(sensorData);

            DataInQueue(sensorData);

            return _Queue.Count;
        }

        public event DataInQueueEventHandler OnDataInQueue;

        protected virtual void DataInQueue(SensorDataContract data)
        {
            DataInQueueEventHandler newData = OnDataInQueue;

            if (newData != null)
            {
                Task.Run(() => newData(data));
            }

            LogMessageReceived( );
        }

        int _receivedMessages = 0;
        DateTime _start;
        private void LogMessageReceived()
        {
            int sent = Interlocked.Increment( ref _receivedMessages );

            if( sent == 1 )
            {
                _start = DateTime.Now;
            }

            if( Interlocked.CompareExchange( ref _receivedMessages, 0, Constants.MessagesLoggingThreshold ) == Constants.MessagesLoggingThreshold )
            {
                DateTime now = DateTime.Now;

                TimeSpan elapsed = ( now - _start );

                _start = now;

                Task.Run( ( ) =>
                {
                    Logger.LogInfo(
                        String.Format( "GatewayService received {0} events succesfully in {1} ms ", Constants.MessagesLoggingThreshold, elapsed.TotalMilliseconds.ToString( ) )
                        );
                } );
            }
        }
    }
}
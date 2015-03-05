namespace Microsoft.ConnectTheDots.Gateway
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ConnectTheDots.Common;

    //--//

    public class GatewayService : IGatewayService
    {
        public delegate void DataInQueueEventHandler( QueuedItem data );

        private readonly IAsyncQueue<QueuedItem> _Queue;
        private readonly EventProcessor _EventProcessor;

        private readonly Func<string, QueuedItem> _DataTransform;

        public GatewayService( IAsyncQueue<QueuedItem> queue, EventProcessor processor, Func<string, QueuedItem> dataTransform = null )
        {
            if( queue == null || processor == null )
            {
                throw new ArgumentException( "Task queue and event processor cannot be null" );
            }

            if( dataTransform != null )
            {
                _DataTransform = dataTransform;
            }
            else
            {
                _DataTransform = m => new QueuedItem
                    {
                        JsonData = m
                    };
            }

            _Queue = queue;
            _EventProcessor = processor;
        }

        public ILogger Logger { get; set; }

        public int Enqueue( string jsonData )
        {
            Logger.LogInfo( "Received from sensor" );

            if( jsonData != null )//not filling a queue by empty items
            {
                QueuedItem sensorData = _DataTransform( jsonData );

                if( sensorData != null )
                {
                    //TODO: we can check status of BatchSender and indicate error on request if needed
                    _Queue.Push( sensorData );

                    DataInQueue( sensorData );
                }
            }

            return _Queue.Count;
        }

        public event DataInQueueEventHandler OnDataInQueue;

        protected virtual void DataInQueue( QueuedItem data )
        {
            DataInQueueEventHandler newData = OnDataInQueue;

            if( newData != null )
            {
                var sh = new SafeAction<QueuedItem>( d => newData( d ), Logger );

                Task.Run( ( ) => sh.SafeInvoke( data ) );
            }

            LogMessageReceived( );
        }

        int _receivedMessages = 0;
        DateTime _start;
        private void LogMessageReceived( )
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

                var sh = new SafeAction<String>( s => Logger.LogInfo( s ), Logger );

                Task.Run( ( ) => sh.SafeInvoke(
                    String.Format( "GatewayService received {0} events succesfully in {1} ms ", Constants.MessagesLoggingThreshold, elapsed.TotalMilliseconds.ToString( ) ) ) );
            }
        }
    }
}
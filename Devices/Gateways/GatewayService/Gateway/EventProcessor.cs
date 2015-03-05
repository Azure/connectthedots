namespace Microsoft.ConnectTheDots.Gateway
{
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Microsoft.ConnectTheDots.Common;

    //--//

    public abstract class EventProcessor
    {
        public delegate void EventBatchProcessedEventHandler( List<Task> messages );

        public abstract bool Start( );

        public abstract bool Stop( int timeout );

        public abstract void Process( );

        public ILogger Logger { get; set; }
    }
}

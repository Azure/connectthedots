using System.Threading.Tasks;
using Gateway.Utils.Logger;
using System.Collections.Generic;
using SharedInterfaces;

namespace Gateway
{
    public abstract class EventProcessor
    {
        public delegate void EventBatchProcessedEventHandler( List<Task> messages );
     
        public abstract bool Start();

        public abstract bool Stop(int timeout);

        public abstract void Process();
        
        public ILogger Logger { get; set; }
    }
}

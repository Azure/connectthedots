//#define DEBUG_LOG
//#define CREATE_CONSUMER_GROUP

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace WebService
{
    class EventHubReader
    {
        private EventHubReceiver[] _receivers;
        private Action<string> _EventAction;
        private readonly string _consumerGroupPrefix;
        private Task[] _tasks;

        public EventHubReader(string consumerGroupPrefix = "Local")
        {
            _consumerGroupPrefix = consumerGroupPrefix;
        }

        public void Close()
        {
            try
            {
                if (_receivers != null)
                {
                    foreach (var r in _receivers)
                    {
                        r.CloseAsync();
                    }
                }
            }
            catch { }
        }

        public void Run(string connectionString, string hubName, Action<string> eventAction)
        {
            if (eventAction == null)
            {
                throw new Exception("eventAction should not be null.");
            }
            _EventAction = eventAction;
            
            NamespaceManager nsmgr = NamespaceManager.CreateFromConnectionString(connectionString);
            EventHubDescription desc = nsmgr.GetEventHub(hubName);

#if CREATE_CONSUMER_GROUP
            string consumerGroupName = _consumerGroupPrefix + DateTime.UtcNow.Ticks;
            //ConsumerGroupDescription consumerGroupDesc = nsmgr.CreateConsumerGroupIfNotExists(new ConsumerGroupDescription(hubName, consumerGroupName));
#endif

            EventHubClient client = EventHubClient.CreateFromConnectionString(connectionString, hubName);

            int numPartitions = desc.PartitionCount;
            _receivers = new EventHubReceiver[numPartitions];

            _tasks = new Task[numPartitions];

            for (int iPart = 0; iPart < desc.PartitionCount; iPart++)
            {
#if CREATE_CONSUMER_GROUP
                EventHubReceiver receiver = client.GetConsumerGroup(consumerGroupName).CreateReceiver(
                    desc.PartitionIds[iPart], DateTime.UtcNow - TimeSpan.FromMinutes(2));
#else
                EventHubReceiver receiver = client.GetDefaultConsumerGroup().CreateReceiver(
                    desc.PartitionIds[iPart], DateTime.UtcNow - TimeSpan.FromMinutes(21));
#endif
                _receivers[iPart] = receiver;

                Task<IEnumerable<EventData>> task = receiver.ReceiveAsync(1000, TimeSpan.FromSeconds(1));

                int thisPart = iPart;
                task.ContinueWith(t => OnTaskComplete(t, thisPart));
                _tasks[iPart] = task;
            }
        }

        void Process(int iPart, bool firstReport, IEnumerable<EventData> batch)
        {
            UTF8Encoding enc = new UTF8Encoding();
            foreach (EventData eventData in batch)
            {
                string body = enc.GetString(eventData.GetBytes());
                string[] lines = body.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    try
                    {
                        _EventAction(line);
                    }
                    catch (Exception)
                    {
#if DEBUG_LOG
                        Trace.TraceError("Ignored invalid event data: {0}", line);
#endif
                    }
                }
            }
        }

        void OnTaskComplete(Task<IEnumerable<EventData>> task, int iPart)
        {
            try
            {
                if (task.IsCompleted)
                {
                    IEnumerable<EventData> batch = task.Result;

                    if (batch != null && batch.Count() != 0)
                    {
                        Process(iPart, false, batch);
                    }
                }
                else
                {
#if DEBUG_LOG
                    Trace.TraceError("Event hub reader {0} did not complete successfully : {1}", iPart,
                        task.Exception == null ? "" : task.Exception.ToString());
#endif
                }

                Task<IEnumerable<EventData>> newTask = _receivers[iPart].ReceiveAsync(1000, TimeSpan.FromSeconds(1));
                int thisPart = iPart;
                newTask.ContinueWith(t => OnTaskComplete(t, thisPart));
                _tasks[iPart] = newTask;
            }
            catch (Exception e)
            {
#if DEBUG_LOG
                Trace.TraceError(e.ToString());
#endif
            }
        }
    }
}

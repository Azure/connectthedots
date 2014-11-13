//  ---------------------------------------------------------------------------------
//  Copyright (c) Microsoft Open Technologies, Inc.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  ---------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Microsoft.ServiceBus.Messaging;

using Newtonsoft.Json;

namespace WebClient
{
    class WebSocketEventProcessor : IEventProcessor
    {
        // Keep track of devices seen, and the last message received for each device
        public static ConcurrentDictionary<string, IDictionary<string, object>> g_devices = 
            new ConcurrentDictionary<string, IDictionary<string, object>>();

        // Keep a buffer of all messages for as long as the client UX needs them
        TimeSpan bufferTimeInterval = new TimeSpan(0, 10, 0);

        // Message buffer (one per processor instance)
        public List<IDictionary<string, object>> bufferedMessages = new List<IDictionary<string, object>>();


        // Remember one event until it is outside of the buffer time period
        EventData eventForNextCheckpoint = null;
        // Remember the index of the checkpoint event in the buffer (to avoid searching for it)
        int indexOfLastCheckpoint = -1;

        // Throttle checkpointing to at most 5 per second (per processor instance)
        TimeSpan maxCheckpointFrequency = new TimeSpan(0, 0, 5);

        // Remember when we last checkpointed (used for checkpoint throttling)
        DateTime lastCheckPoint = DateTime.MinValue;


        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> events)
        {
            try
            {
                var now = DateTime.UtcNow;

                foreach (var eventData in events)
                {
                    // Get message from the eventData body and convert JSON string into message object
                    string eventBodyAsString = Encoding.UTF8.GetString(eventData.GetBytes());

                    IList<IDictionary<string, object>> messagePayloads;
                    try
                    {
                        // Attempt to deserialze event body as single JSON message
                        messagePayloads = new List<IDictionary<string, object>> 
                        { 
                            JsonConvert.DeserializeObject<IDictionary<string, object>>(eventBodyAsString)
                        };
                    }
                    catch
                    {
                        // Not a single JSON message: attempt to deserialize as array of messages
                        
                        // Azure Stream Analytics Preview generates invalid JSON for some multi-values queries
                        // Workaround: turn concatenated json objects (ivalid JSON) into array of json objects (valid JSON)
                        if (eventBodyAsString.IndexOf("}{") >= 0)
                        {
                            eventBodyAsString = eventBodyAsString.Replace("}{", "},{");
                            eventBodyAsString = "[" + eventBodyAsString + "]";
                        }
                        messagePayloads = JsonConvert.DeserializeObject<IList<IDictionary<string, object>>>(eventBodyAsString);
                    }

                    // Only send messages within the display/buffer interval to clients, to speed up recovery after downtime
                    if ((eventData.EnqueuedTimeUtc + bufferTimeInterval).AddMinutes(5) > now)
                    {
                        foreach (var messagePayload in messagePayloads)
                        {
                            // Build up the list of devices seen so far (in lieu of a formal device repository)
                            // Also keep the last message received per device (not currently used in the sample)
                            string deviceName = null;
                            if (messagePayload.ContainsKey("dspl"))
                            {
                                deviceName = messagePayload["dspl"] as string;
                                if (deviceName != null)
                                {
                                    WebSocketEventProcessor.g_devices.TryAdd(deviceName, messagePayload);
                                }
                            }
                            
                            // Notify clients
                            MyWebSocketHandler.SendToClients(messagePayload);

                            // Buffer messages so we can resend them to clients that connect later
                            // or when a client requests data for a different device

                            // Lock to guard against concurrent reads from client resend
                            // Note that the Add operations are not contentious with each other 
                            // because EH processor host serializes per partition, and we use one buffer per partition
                            lock (bufferedMessages)
                            {
                                bufferedMessages.Add(messagePayload);
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Received event older than {0} in EH {1}, partition {2}: {3} - Sequence Number {4}", 
                            bufferTimeInterval, 
                            context.EventHubPath, context.Lease.PartitionId, 
                            eventData.EnqueuedTimeUtc, eventData.SequenceNumber);

                        eventForNextCheckpoint = eventData;
                    }

                    // Remember first event to checkpoint to later
                    if (eventForNextCheckpoint == null)
                    {
                        eventForNextCheckpoint = eventData;
                    }
                }

                // Checkpoint to an event before the buffer/display time period, so we can recover the events on VM restart
                if (eventForNextCheckpoint != null
                    && eventForNextCheckpoint.EnqueuedTimeUtc + bufferTimeInterval < now
                    && lastCheckPoint + maxCheckpointFrequency < now) // Don't checkpoint too often, as every checkpoint incurs at least one blob storage roundtrip
                {
                    await context.CheckpointAsync(eventForNextCheckpoint);

                    Trace.TraceInformation("Checkpointed EH {0}, partition {1}: offset {2}, Sequence Number {3}, time {4}",
                        context.EventHubPath, context.Lease.PartitionId, 
                        eventForNextCheckpoint.Offset, eventForNextCheckpoint.SequenceNumber, 
                        eventForNextCheckpoint.EnqueuedTimeUtc);

                    // Remove all older messages from the resend buffer
                    lock (bufferedMessages)
                    {
                        if (this.indexOfLastCheckpoint >= 0)
                        {
                            bufferedMessages.RemoveRange(0, this.indexOfLastCheckpoint);
                        }
                        indexOfLastCheckpoint = bufferedMessages.Count - 1;
                    }

                    // Get ready for next checkpoint
                    lastCheckPoint = now;
                    eventForNextCheckpoint = events.Last<EventData>();
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Error processing events in EH {0}, partition {1}: {0}", 
                    context.EventHubPath, context.Lease.PartitionId, e.Message);
            }
        }

        public Task OpenAsync(PartitionContext context)
        {
            Trace.TraceInformation(
                String.Format("Opening processor for EH {0}, partition {1}.",
                    context.EventHubPath, context.Lease.PartitionId));
            lock (g_processors)
            {
                try
                {
                    g_processors.Add(this);
                }
                catch (Exception e)
                {
                    Trace.TraceError("Exception while adding processor for EH {0}, partition {1}: {2}",
                        context.EventHubPath, context.Lease.PartitionId, e.Message);
                }
            }
            return Task.FromResult<object>(null);
        }

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Trace.TraceInformation(
                String.Format("Closing processor for EH {0}, partition {1}. Reason: {2}",
                    context.EventHubPath, context.Lease.PartitionId, reason));

            lock (g_processors)
            {
                try
                {
                    g_processors.Remove(this);
                }
                catch (Exception e)
                {
                    Trace.TraceError(
                        String.Format("Exception while removing processor for EH {0}, partition {1}: {2}",
                            context.EventHubPath, context.Lease.PartitionId, e.Message));
                }
            }
            return Task.FromResult<object>(null);
        }

        public static void ExceptionReceived(object sender, ExceptionReceivedEventArgs e)
        {
            Trace.TraceError("Exception received from EventHostProcessor: {0} - {1}, {2}", e.Exception, e.Action, sender);
        }

        static List<WebSocketEventProcessor> g_processors = new List<WebSocketEventProcessor>();

        // Retrieve buffered messages from all EH partitions (= processor instances)
        // TODO: This needs to be partitioned and/or turned into a distributed call/cache 
        //  to support effective scale-out to multiple web client machines/VMs for large number of devices
        public static IEnumerable<IDictionary<string, object>> GetAllBufferedMessages()
        {
            var allMessages = new List<IDictionary<string, object>>();
            lock (g_processors)
            {
                foreach (var processor in g_processors)
                {
                    lock (processor.bufferedMessages)
                    {
                        allMessages.AddRange(processor.bufferedMessages);
                    }
                }
            }
            return allMessages;
        }

    }
}

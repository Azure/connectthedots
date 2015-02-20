using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Gateway.Utils.MessageSender;
using Gateway.Utils.OperationStatus;
using System.Diagnostics;

namespace Gateway.Utils.Queue
{
    public class BatchSenderThread<TQueueItem, TMessage> : EventProcessor
    {
        private readonly IAsyncQueue<TQueueItem> _DataSource;
        private readonly IMessageSender<TMessage> _DataTarget;
        private readonly Func<TQueueItem, TMessage> _DataTransform;
        private readonly Func<TQueueItem, string> _SerializedData;
        private int _outstandingTasks;

        private Thread _WorkerThread;
        private AutoResetEvent _operational;
        private AutoResetEvent _doWork;
        private bool _running; 

        private object _SyncRoot = new object();

        private static readonly string _LogMessagePrefix = "BatchSenderThread error. ";

        public BatchSenderThread(IAsyncQueue<TQueueItem> dataSource, IMessageSender<TMessage> dataTarget, Func<TQueueItem, TMessage> dataTransform, Func<TQueueItem, string> serializedData)
        {
            if (dataSource == null || dataTarget == null)
            {
                throw new ArgumentException("data source and data target cannot be null");
            }

            _operational = new AutoResetEvent(false);
            _doWork = new AutoResetEvent(false);
            _running = false;
            _DataSource = dataSource;
            _DataTarget = dataTarget;
            _DataTransform = dataTransform;
            _SerializedData = serializedData;
            _outstandingTasks = 0;
        }

        public override bool Start()
        {
            bool start = false;

            lock (_SyncRoot)
            {
                if (_running == false)
                {
                    start = true;
                }
            }

            if (start)
            {
                _WorkerThread = new Thread(ThreadJob);
                _running = true;
                _WorkerThread.Start();
                return _operational.WaitOne();
            }

            return false;
        }

        public override bool Stop(int timeout)
        {
            bool stop = false;

            lock(_SyncRoot)
            {
                if (_running == true)
                {
                    // There must exist a worker thread
                    System.Diagnostics.Debug.Assert(_WorkerThread != null);

                    // signal the worker thread that exit is impending
                    _running = false;
                    _doWork.Set();

                    stop = true;
                }
            }

            if (stop) 
            {
                if (_operational.WaitOne(timeout) == false)
                {
                    // no other choice than forcing a stop
                    _WorkerThread.Abort();
                }

                _WorkerThread.Join();

                return true;
            }

            return false;
        }

        public override void Process()
        {
            _doWork.Set();
        }

        public event EventBatchProcessedEventHandler OnEventsBatchProcessed;

        private void ThreadJob()
        {
            // signal that the worker thread has actually started processing the events
            _operational.Set();

            try
            {
                const int WAIT_TIMEOUT = 50;

                // run until Stop() is called
                while (_running == true)
                {
                    try
                    {
                        // If there are no tasks to be served, wait for some events to process
                        // Use a timeout to prevent race conditions on teh outstanding tasks count
                        // and the actual queue count
                        _doWork.WaitOne( WAIT_TIMEOUT );

                        // Fish from the queue and accumulate, keep track of outstanding tasks to 
                        // avoid accumulating too many competing tasks. Note that we are going to schedule
                        // one more tasks than strictly needed, so that we prevent tasks to sit in the queue
                        // because of the race condition on the outstanding task count (_outstandingTasks) 
                        // and the tasks actually sitting in the queue.  (*)
                        // To prevent this race condition, we will wait with a timeout
                        int count = _DataSource.Count - _outstandingTasks;

                        if(count == 0)
                        {
                            continue;
                        }

                        // check if we have been woken up to actually stop processing 
                        EventBatchProcessedEventHandler eventBatchProcessed = null;

                        lock (_SyncRoot)
                        {
                            if (_running == false)
                            {
                                return;
                            }

                            // take a snapshot of event handlers to invoke
                            eventBatchProcessed = OnEventsBatchProcessed;
                        }

                        // allocate a container to keep track of tasks for events in the queue
                        var tasks = new List<Task>();

                        // process all messages that have not been processed yet 
                        while( --count >= 0 )
                        {
                            var t = _DataSource.TryPop( );

                            // increment outstanding task count 
                            Interlocked.Increment( ref _outstandingTasks );

                            t.ContinueWith<Task>( popped =>
                                    {
                                        // Decrement the numbers of outstanding tasks. 
                                        // (*) Note that there is a race  condition because at this point in time the tasks 
                                        // is already out of the queue but we did not decrement the outstanding task count 
                                        // yet. This race condition may cause tasks to be left sitting in the queue. 
                                        // To deal with this race condition, we will wait with a timeout
                                        Interlocked.Decrement( ref _outstandingTasks );

                                        // because the outstanding task counter is incremented before 
                                        // adding, we should never incur a negative count 
                                        Debug.Assert( _outstandingTasks >= 0 );

                                        if( popped.Result.IsSuccess )
                                        {
                                            if( _DataTransform != null )
                                            {
                                                return _DataTarget.SendMessage( _DataTransform( popped.Result.Result ) );
                                            }
                                            if( _SerializedData != null )
                                            {
                                                return _DataTarget.SendSerialized( _SerializedData( popped.Result.Result ) );
                                            }

                                            Debug.Assert( false );
                                        }

                                        return popped;
                                    } );

                            try
                            {
                                tasks.Add( t );
                            }
                            catch( StackOverflowException ex )
                            {
                                // do not hide stack overflow exceptions
                                throw;
                            }
                            catch( OutOfMemoryException ex )
                            {
                                // do not hide memory exceptions
                                throw;
                            }
                            catch
                            {
                                // We should try and recover for Task 't' that we popped and did not process 
                                if (t.Status == TaskStatus.RanToCompletion || t.Status == TaskStatus.Running)
                                {
                                    //We need to wait for a result if task was started
                                    try
                                    {
                                        OperationStatus<TQueueItem> popResult = t.Result;
                                        if (popResult.IsSuccess)
                                        {
                                            //we don't want possibly race with other sending tasks
                                            //more safety would be just give message back to the queue
                                            _DataSource.Push(popResult.Result);
                                        }
                                    }
                                    catch (AggregateException) { }
                                }

                                throw;
                            }
                        }
                        
                        // alert any client about outstanding message tasks
                        if (eventBatchProcessed != null)
                        {
                            Task.Run(() =>
                                {
                                    eventBatchProcessed(tasks);
                                });
                        }
                    }
                    catch (StackOverflowException ex)
                    {
                        if (Logger != null)
                        {
                            Logger.LogError(_LogMessagePrefix + ex.Message);
                        }

                        // do not hide stack overflow exceptions
                        throw;
                    }
                    catch (OutOfMemoryException ex)
                    {
                        if (Logger != null)
                        {
                            Logger.LogError(_LogMessagePrefix + ex.Message);
                        }

                        // do not hide memory exceptions
                        throw;
                    }
                    catch (Exception ex)
                    {
                        if (Logger != null)
                        {
                            Logger.LogError(_LogMessagePrefix + ex.Message);
                        }

                        // catch all other exceptions
                    }

                    // go and check for more events
                }
            }
            finally
            {
                // signal stop
                _operational.Set();
            }
        }
    }
}

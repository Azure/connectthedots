namespace Microsoft.ConnectTheDots.Gateway
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.ConnectTheDots.Common;

    //--//

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

        public BatchSenderThread(IAsyncQueue<TQueueItem> dataSource, IMessageSender<TMessage> dataTarget, Func<TQueueItem, TMessage> dataTransform, Func<TQueueItem, string> serializedData, ILogger logger)
        {
            Logger = SafeLogger.FromLogger( logger );

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
                const int WAIT_TIMEOUT = 50; // millisecods

                // run until Stop() is called
                while (_running == true)
                {
                    try
                    {
                        // If there are no tasks to be served, wait for some events to process
                        // Use a timeout to prevent race conditions on teh outstanding tasks count
                        // and the actual queue count
                        _doWork.WaitOne(WAIT_TIMEOUT);

                        // Fish from the queue and accumulate, keep track of outstanding tasks to 
                        // avoid accumulating too many competing tasks. Note that we are going to schedule
                        // one more tasks than strictly needed, so that we prevent tasks to sit in the queue
                        // because of the race condition on the outstanding task count (_outstandingTasks) 
                        // and the tasks actually sitting in the queue.  (*)
                        // To prevent this race condition, we will wait with a timeout
                        int count = _DataSource.Count - _outstandingTasks;

                        if (count == 0)
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
                        while (--count >= 0)
                        {
                            var t = _DataSource.TryPop();

                            // increment outstanding task count 
                            Interlocked.Increment(ref _outstandingTasks);

                            t.ContinueWith<Task>(popped =>
                            {
                                try
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

                                    if( popped.Result.IsSuccess && popped.Result != null )
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
                                }
                                catch( StackOverflowException ex )
                                {
                                    Logger.LogError( _LogMessagePrefix + ex.Message );

                                    // do not hide stack overflow exceptions
                                    throw;
                                }
                                catch( OutOfMemoryException ex )
                                {
                                    Logger.LogError( _LogMessagePrefix + ex.Message );

                                    // do not hide memory exceptions
                                    throw;
                                }
                                catch( Exception ex )
                                {
                                    Logger.LogError( _LogMessagePrefix + ex.Message );

                                    // catch all other exceptions
                                }

                                return popped;
                            });

                            AddToProcessed(tasks, t);
                        }

                        // alert any client about outstanding message tasks
                        if (eventBatchProcessed != null)
                        {
                            var sh = new SafeAction<List<Task>>( t => eventBatchProcessed( t ), Logger );

                            Task.Run( () => sh.SafeInvoke( tasks ) ); 
                        }
                    }
                    catch (StackOverflowException ex)
                    {
                        Logger.LogError(_LogMessagePrefix + ex.Message);

                        // do not hide stack overflow exceptions
                        throw;
                    }
                    catch (OutOfMemoryException ex)
                    {
                        Logger.LogError(_LogMessagePrefix + ex.Message);

                        // do not hide memory exceptions
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(_LogMessagePrefix + ex.Message);

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

        private void AddToProcessed( List<Task> tasks, Task<OperationStatus<TQueueItem>> t )
        {
            try
            {
                tasks.Add( t );
            }
            catch( StackOverflowException /*ex*/)
            {
                // do not hide stack overflow exceptions
                throw;
            }
            catch( OutOfMemoryException /*ex*/)
            {
                // do not hide memory exceptions
                throw;
            }
            catch( Exception ex )
            {
                Logger.LogError( "Exception on adding task: " + ex.Message );

                // catch all other exceptions

                //
                // TODO
                // If we are here, the task that has been popped could not be added to the list
                // of tasks that the client will be notifed about
                // This does not mean that the task has not been processed though
                //
            }
        }
    }
}

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

//#define USE_TASKS

namespace Microsoft.ConnectTheDots.Common.Threading
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
#if !USE_TASKS
    using System.Threading;
#endif
    using _THREADING = System.Threading.Tasks;

    //--//

    public enum TaskStatus
    {
        // Summary:
        //     The task has been initialized but has not yet been scheduled.
        Created = 0,
        //
        // Summary:
        //     The task is waiting to be activated and scheduled internally by the .NET
        //     Framework infrastructure.
        WaitingForActivation = 1,
        //
        // Summary:
        //     The task has been scheduled for execution but has not yet begun executing.
        WaitingToRun = 2,
        //
        // Summary:
        //     The task is running but has not yet completed.
        Running = 3,
        //
        // Summary:
        //     The task has finished executing and is implicitly waiting for attached child
        //     tasks to complete.
        WaitingForChildrenToComplete = 4,
        //
        // Summary:
        //     The task completed execution successfully.
        RanToCompletion = 5,
        //
        // Summary:
        //     The task acknowledged cancellation by throwing an OperationCanceledException
        //     with its own CancellationToken while the token was in signaled state, or
        //     the task's CancellationToken was already signaled before the task started
        //     executing. For more information, see Task Cancellation.
        Canceled = 6,
        //
        // Summary:
        //     The task completed due to an unhandled exception.
        Faulted = 7,
    }

#if USE_TASKS
    public class TaskWrapper
    {
        private _THREADING.Task _t;

        //--//

        public static TaskWrapper Run( Action action )
        {
            var t = new TaskWrapper( action );

            t.Start();

            return t;
        }

        public static void WaitAll( params TaskWrapper[] tasks )
        {
            _THREADING.Task[] ts = new _THREADING.Task[ tasks.Length ];
            
            for( int i = 0; i < tasks.Length; ++i )
            {
                ts[ i ] = tasks[ i ].InnerTask;
            }

            _THREADING.Task.WaitAll( ts );
        }

        //--//

        protected TaskWrapper( )
        {
        }

        protected TaskWrapper( Action action )
        {
            _t = new _THREADING.Task( action );
        }

        public TaskWrapper( _THREADING.Task t )
        {
            Debug.Assert( t != null );
            _t = t;
        }

        public void Start()
        {
            _t.Start( );
        }

        public void Wait()
        {
            _t.Wait( );
        }

        public int Id
        {
            get
            {
                return _t.Id;
            }
        }

        public TaskStatus Status
        {
            get
            {
                return (TaskStatus)_t.Status;
            }
        }

        protected _THREADING.Task InnerTask
        {
            get
            {
                return _t;
            }
            set
            {
                _t = value;
            }
        }


    }

    public class TaskWrapper<TResult> : TaskWrapper
    {
        private readonly _THREADING.Task<TResult> _t;

        //--//

        public static TaskWrapper<TResult> Run( Func<TResult> function )
        {
            var t = new TaskWrapper<TResult>( function );

            t.Start();

            return t;
        }

        //--//

        private static Action MakeDefault<T>( Func<T> function )
        {
            return () => { function(); };
        }

        public TaskWrapper( Func<TResult> function )
            : base( )
        {
            _t = new _THREADING.Task<TResult>( function );

            InnerTask = _t;
        }

        private TaskWrapper( _THREADING.Task<TResult> t )
            : base ( t )
        {
        }
            
        public TaskWrapper<TNewResult> ContinueWith<TNewResult>( Func<_THREADING.Task<TResult>, TNewResult> continuationFunction )
        {
            return new TaskWrapper<TNewResult>( _t.ContinueWith<TNewResult>( continuationFunction ) );
        }

        public TResult Result
        {
            get
            {
                return _t.Result;
            }
        }
    }
#else

    public class TaskWrapper
    {
        private static int _unique_id = 0;

        //--//

        private readonly int                _id;
        private          TaskStatus         _status;
        private          ManualResetEvent   _completed;

        //--//

        protected readonly Action        _action;

        //--//

        public static TaskWrapper Run( Action action )
        {
            var t = new TaskWrapper( action );

            t.Start( );

            return t;
        }

        public static void WaitAll( params TaskWrapper[] tasks )
        {
            WaitHandle[] wh = new WaitHandle[ tasks.Length ];

            for( int i = 0; i < tasks.Length; ++i )
            {
                wh[ i ] = tasks[ i ]._completed;
            }

            AutoResetEvent.WaitAll( wh, Timeout.Infinite );
        }

        //--//

        protected TaskWrapper( )
        {
            _id = Interlocked.Increment( ref _unique_id );
            _status = TaskStatus.Created;
            _completed = new ManualResetEvent( false );
        }

        protected TaskWrapper( Action action )
            : this( )
        {
            _action = action;
        }

        public virtual void Start( )
        {
            ThreadPool.QueueUserWorkItem( Execute );
        }

        public void Wait( )
        {
            _completed.WaitOne( );
        }

        public int Id
        {
            get
            {
                return _id;
            }
        }

        public TaskStatus Status
        {
            get
            {
                return _status;
            }
        }

        protected void SetStatus( TaskStatus status )
        {
            _status = status;
        }

        protected bool IsRunningOrDone( )
        {
            return _status == TaskStatus.WaitingToRun ||
                   _status == TaskStatus.Running ||
                   _status == TaskStatus.Faulted ||
                   _status == TaskStatus.RanToCompletion;
        }

        protected void SetCompleted( )
        {
            _completed.Set( );
        }

        protected void WaitCompleted( )
        {
            _completed.WaitOne( );
        }

        private void Execute( object state )
        {
            _status = TaskStatus.Running;

            try
            {
                _action( );
            }
            catch
            {
                _status = TaskStatus.Faulted;
            }

            _status = TaskStatus.RanToCompletion;

            _completed.Set( );
        }
    }

    public class TaskWrapper<TResult> : TaskWrapper
    {
        private          Func<TResult> _func;
        private          object        _cont;
        private          TResult       _result   = default( TResult );
        private readonly object        _syncRoot = new object( );

        //--//

        public static TaskWrapper<TResult> Run( Func<TResult> function )
        {
            var t = new TaskWrapper<TResult>( function );

            t.Start( );

            return t;
        }

        //--//

        private TaskWrapper( Func<TResult> func )
            : base( )
        {
            _func = func;
        }

        public override void Start( )
        {
            ThreadPool.QueueUserWorkItem( Execute );
        }

        private TaskWrapper<TOutput> MakeTask<TInput, TOutput>( Func<TaskWrapper<TResult>, TOutput> continuationFunction )
        {
            return new TaskWrapper<TOutput>( ( ) =>
            {
                return continuationFunction( this );
            } );
        }

        public TaskWrapper<TNewResult> ContinueWith<TNewResult>( Func<TaskWrapper<TResult>, TNewResult> continuationFunction )
        {
            _cont = MakeTask<TaskWrapper<TResult>, TNewResult>( continuationFunction );

            lock( _syncRoot )
            {
                if( IsRunningOrDone( ) )
                {
                    // Task is executing or done, schedule againto 
                    // make sure continuation will be served
                    Start( );
                }
            }

            return (TaskWrapper<TNewResult>)_cont;
        }

        public TResult Result
        {
            get
            {
                return _result;
            }
        }

        private void Execute( object state )
        {
            //
            // we want to execute _func only once 
            //
            Func<TResult> f = null;
            lock( _syncRoot )
            {
                if( _func != null )
                {
                    f = _func;

                    _func = null;

                    SetStatus( TaskStatus.WaitingToRun );
                }
            }

            if( f != null )
            {
                try
                {
                    SetStatus( TaskStatus.Running );

                    _result = f( );
                }
                catch
                {
                    SetStatus( TaskStatus.Faulted );
                }

                SetStatus( TaskStatus.RanToCompletion );

                SetCompleted( );
            }

            //
            // we want to execute _cont only once 
            //
            TaskWrapper cont = null;
            lock( _syncRoot )
            {
                if( _cont != null )
                {
                    cont = (TaskWrapper)_cont;

                    _cont = null;
                }
            }

            if( cont != null )
            {
                // do not start the continuation before the task is completed
                WaitCompleted( );

                ( (TaskWrapper)cont ).Start( );
            }
        }
    }
#endif
}

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

namespace Microsoft.ConnectTheDots.Common.Threading
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
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

    public class TaskWrapper
    {
        //private readonly int             _id;
        //private          TaskStatus      _status;
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

        TResult _result = default( TResult );

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
                return _result;
            }
        }
    }
}

/**
 * 
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Threading.Tasks
{
    [DebuggerStepThrough]
    public static class TaskHelper
    {
        #region Task<TResult> Run<TResult>
        public static Task<TResult> Run<TResult>(Func<TResult> func)
        {
            Tuple<Func<TResult>> tuple = new Tuple<Func<TResult>>(func);

            return Task.Factory.StartNew((Func<object, TResult>)InvokeFunc<TResult>, tuple, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        public static Task<TResult> Run<TResult>(Func<TResult> func, CancellationToken token)
        {
            Tuple<Func<TResult>> tuple = new Tuple<Func<TResult>>(func);

            return Task.Factory.StartNew((Func<object, TResult>)InvokeFunc<TResult>, tuple, token, TaskCreationOptions.None, TaskScheduler.Default);
        }

        public static Task<TResult> Run<TResult>(Func<TResult> func, CancellationToken token, TaskCreationOptions options)
        {
            Tuple<Func<TResult>> tuple = new Tuple<Func<TResult>>(func);

            return Task.Factory.StartNew((Func<object, TResult>)InvokeFunc<TResult>, tuple, token, options, TaskScheduler.Default);
        }

        public static Task<TResult> Run<TResult>(Func<TResult> func1, CancellationToken token, TaskCreationOptions options, TaskScheduler taskScheduler)
        {
            Tuple<Func<TResult>> tuple = new Tuple<Func<TResult>>(func1);

            return Task.Factory.StartNew((Func<object, TResult>)InvokeFunc<TResult>, tuple, token, options, taskScheduler);
        }
        
        #endregion

        #region Task<TResult> Run<T1, TResult>
        public static Task<TResult> Run<T1, TResult>(Func<T1, TResult> func, T1 o1)
        {
            Tuple<Func<T1, TResult>, T1> tuple = new Tuple<Func<T1, TResult>, T1>(func, o1);

            return Task.Factory.StartNew((Func<object, TResult>)InvokeFunc<T1, TResult>, tuple, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        public static Task<TResult> Run<T1, TResult>(Func<T1, TResult> func, T1 o1, CancellationToken token)
        {
            Tuple<Func<T1, TResult>, T1> tuple = new Tuple<Func<T1, TResult>, T1>(func, o1);

            return Task.Factory.StartNew((Func<object, TResult>)InvokeFunc<T1, TResult>, tuple, token, TaskCreationOptions.None, TaskScheduler.Default);
        }

        public static Task<TResult> Run<T1, TResult>(Func<T1, TResult> func, T1 o1, CancellationToken token, TaskCreationOptions options)
        {
            Tuple<Func<T1, TResult>, T1> tuple = new Tuple<Func<T1, TResult>, T1>(func, o1);

            return Task.Factory.StartNew((Func<object, TResult>)InvokeFunc<T1, TResult>, tuple, token, options, TaskScheduler.Default);
        }

        public static Task<TResult> Run<T1, TResult>(Func<T1, TResult> func, T1 o1, CancellationToken token, TaskCreationOptions options, TaskScheduler taskScheduler)
        {
            Tuple<Func<T1, TResult>, T1> tuple = new Tuple<Func<T1, TResult>, T1>(func, o1);

            return Task.Factory.StartNew((Func<object, TResult>)InvokeFunc<T1, TResult>, tuple, token, options, taskScheduler);
        }

        #endregion

        [DebuggerStepThrough]
        private static TResult InvokeFunc<T1, TResult>(object o)
        {
            return ((Tuple<Func<T1, TResult>, T1>)o).Item1(((Tuple<Func<T1, TResult>, T1>)o).Item2);
        }

        [DebuggerStepThrough]
        private static TResult InvokeFunc<TResult>(object o)
        {
            return ((Tuple<Func<TResult>>)o).Item1();
        }
    }
}

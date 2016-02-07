/**
 * 
 */

using System.Diagnostics;

namespace System.Threading.Tasks
{
    [DebuggerStepThrough]
    public static class TaskHelper
    {
        public static Task Run<T1, T2, T3>(Action<T1, T2, T3> action, T1 o1, T2 o2, T3 o3, CancellationToken token,
            TaskCreationOptions options, TaskScheduler taskScheduler)
        {
            var tuple = new Tuple<Action<T1, T2, T3>, T1, T2, T3>(action, o1, o2, o3);

            return Task.Factory.StartNew(Invoke<T1, T2, T3>, tuple, token, options, taskScheduler);
        }

        public static Task Run<T1, T2>(Action<T1, T2> action, T1 o1, T2 o2, CancellationToken token,
            TaskCreationOptions options, TaskScheduler taskScheduler)
        {
            var tuple = new Tuple<Action<T1, T2>, T1, T2>(action, o1, o2);

            return Task.Factory.StartNew(Invoke<T1, T2>, tuple, token, options, taskScheduler);
        }
        
        private static void Invoke<T1, T2>(object o)
        {
            ((Tuple<Action<T1, T2>, T1, T2>)o).Item1(
                ((Tuple<Action<T1, T2>, T1, T2>)o).Item2,
                ((Tuple<Action<T1, T2>, T1, T2>)o).Item3);
        }

        private static void Invoke<T1, T2, T3>(object o)
        {
            ((Tuple<Action<T1, T2, T3>, T1, T2, T3>)o).Item1(
                ((Tuple<Action<T1, T2, T3>, T1, T2, T3>)o).Item2,
                ((Tuple<Action<T1, T2, T3>, T1, T2, T3>)o).Item3,
                ((Tuple<Action<T1, T2, T3>, T1, T2, T3>)o).Item4);
        }

        [DebuggerStepThrough]
        private static TResult InvokeFunc<T1, TResult>(object o)
        {
            return ((Tuple<Func<T1, TResult>, T1>) o).Item1(((Tuple<Func<T1, TResult>, T1>) o).Item2);
        }

        [DebuggerStepThrough]
        private static TResult InvokeFunc<TResult>(object o)
        {
            return ((Tuple<Func<TResult>>) o).Item1();
        }

        #region Task<TResult> Run<TResult>

        public static Task<TResult> Run<TResult>(Func<TResult> func)
        {
            var tuple = new Tuple<Func<TResult>>(func);

            return Task.Factory.StartNew((Func<object, TResult>) InvokeFunc<TResult>, tuple, CancellationToken.None,
                TaskCreationOptions.None, TaskScheduler.Default);
        }

        public static Task<TResult> Run<TResult>(Func<TResult> func, CancellationToken token)
        {
            var tuple = new Tuple<Func<TResult>>(func);

            return Task.Factory.StartNew((Func<object, TResult>) InvokeFunc<TResult>, tuple, token,
                TaskCreationOptions.None, TaskScheduler.Default);
        }

        public static Task<TResult> Run<TResult>(Func<TResult> func, CancellationToken token,
            TaskCreationOptions options)
        {
            var tuple = new Tuple<Func<TResult>>(func);

            return Task.Factory.StartNew((Func<object, TResult>) InvokeFunc<TResult>, tuple, token, options,
                TaskScheduler.Default);
        }

        public static Task<TResult> Run<TResult>(Func<TResult> func1, CancellationToken token,
            TaskCreationOptions options, TaskScheduler taskScheduler)
        {
            var tuple = new Tuple<Func<TResult>>(func1);

            return Task.Factory.StartNew((Func<object, TResult>) InvokeFunc<TResult>, tuple, token, options,
                taskScheduler);
        }

        #endregion

        #region Task<TResult> Run<T1, TResult>

        public static Task<TResult> Run<T1, TResult>(Func<T1, TResult> func, T1 o1)
        {
            var tuple = new Tuple<Func<T1, TResult>, T1>(func, o1);

            return Task.Factory.StartNew((Func<object, TResult>) InvokeFunc<T1, TResult>, tuple, CancellationToken.None,
                TaskCreationOptions.None, TaskScheduler.Default);
        }

        public static Task<TResult> Run<T1, TResult>(Func<T1, TResult> func, T1 o1, CancellationToken token)
        {
            var tuple = new Tuple<Func<T1, TResult>, T1>(func, o1);

            return Task.Factory.StartNew((Func<object, TResult>) InvokeFunc<T1, TResult>, tuple, token,
                TaskCreationOptions.None, TaskScheduler.Default);
        }

        public static Task<TResult> Run<T1, TResult>(Func<T1, TResult> func, T1 o1, CancellationToken token,
            TaskCreationOptions options)
        {
            var tuple = new Tuple<Func<T1, TResult>, T1>(func, o1);

            return Task.Factory.StartNew((Func<object, TResult>) InvokeFunc<T1, TResult>, tuple, token, options,
                TaskScheduler.Default);
        }

        public static Task<TResult> Run<T1, TResult>(Func<T1, TResult> func, T1 o1, CancellationToken token,
            TaskCreationOptions options, TaskScheduler taskScheduler)
        {
            var tuple = new Tuple<Func<T1, TResult>, T1>(func, o1);

            return Task.Factory.StartNew((Func<object, TResult>) InvokeFunc<T1, TResult>, tuple, token, options,
                taskScheduler);
        }

        #endregion
    }
}
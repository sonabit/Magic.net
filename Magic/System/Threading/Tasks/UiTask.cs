/**
 * This class UiTask is not finnished jet
 * 
 * there is a critical bug, when its called from UI-thread and wait for the returning Task
 * The UI will freeze and never come back.
*/
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace System.Threading.Tasks
{
    /// <summary>
    /// The methods will perform a action or function in UI-thread.
    /// <para> This class UiTask is not finnished jet</para>
    /// </summary>
    /// <remarks><para>This class helps to perfome some actions in UI thread, e.g. to add some entry from a backgroundworker or task to a ObservableCollection, which is bound at a UI-control.</para>
    /// <para>It is necessary to perform the <see cref="UiTask.Initialize"/> onetime from UI-thread before call any method.</para>
    /// <para> there is a critical bug, when its called from UI-thread and wait for the returning Task. The UI will freeze and never come back.</para>
    /// </remarks>
    /// <code>
    /// // Task or BackgroundWorker
    /// foreach (var item in items)
    /// {
    ///     UiTask.Run(collection.Add, item);
    /// }
    /// </code>
    [DebuggerNonUserCode]
    public static class UiTask
    {
        #region Fields

        private static TaskScheduler _uiTaskScheduler;
        private static TaskFactory _uiTaskFactory;

        #endregion Fields
        

        public static void Initialize()
        {
            if (_uiTaskScheduler != null) return;
            _uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            _uiTaskFactory = new TaskFactory(_uiTaskScheduler);
        }

        public static TaskScheduler UiTaskScheduler
        {
            get
            {
                if (_uiTaskScheduler == null) throw new InvalidOperationException("UiTaskScheduler is not initialiezed. Please call UiTask.Initialize() from UI-Thread.");
                return _uiTaskScheduler;
            }
        }

        public static TaskFactory UiTaskFactory
        {
            get
            {
                if (_uiTaskFactory == null) throw new InvalidOperationException("UiTaskFactory is not initialiezed. Please call UiTask.Initialize() from UI-Thread.");
                return _uiTaskFactory;
            }
        }

        public static async Task Run(Action action)
        {
            await UiTaskFactory.StartNew(action);
        }

        public static async Task<T> Run<T>(Func<T> func)
        {
            return await UiTaskFactory.StartNew(func);
        }

        public static async Task Run<T1>(Action<T1> action, T1 obj1)
        {
            Tuple<Action<T1>, T1> tuple = new Tuple<Action<T1>, T1>(action, obj1);

            await UiTaskFactory.StartNew(t => ((Tuple<Action<T1>, T1>)t).Item1(((Tuple<Action<T1>, T1>)t).Item2), tuple);
        }

        public static async Task Run<T1, T2>(Action<T1, T2> action, T1 obj1, T2 obj2)
        {
            Tuple<Action<T1, T2>, T1, T2> tuple = new Tuple<Action<T1, T2>, T1, T2>(action, obj1, obj2);

            await UiTaskFactory.StartNew(t => ((Tuple<Action<T1, T2>, T1, T2>)t).Item1(((Tuple<Action<T1, T2>, T1, T2>)t).Item2, ((Tuple<Action<T1, T2>, T1, T2>)t).Item3), tuple);
        }

        public static async Task RunAsync<T1, T2>(Action<T1, T2> action, T1 obj1, T2 obj2, CancellationToken cancellationToken)
        {
            Tuple<Action<T1, T2>, T1, T2> tuple = new Tuple<Action<T1, T2>, T1, T2>(action, obj1, obj2);

            await UiTaskFactory.StartNew(t => ((Tuple<Action<T1, T2>, T1, T2>)t).Item1(((Tuple<Action<T1, T2>, T1, T2>)t).Item2, ((Tuple<Action<T1, T2>, T1, T2>)t).Item3), tuple, cancellationToken);
        }

        public static async Task RunAsync(Action action)
        {
            await UiTaskFactory.StartNew(action);
        }

        public static async Task Run(Action action, CancellationToken cancellationToken)
        {
            await UiTaskFactory.StartNew(action, cancellationToken);
        }
        
        public static async Task<T> RunInUiAsync<T>(Func<T> function)
        {
            return await UiTaskFactory.StartNew(function); 
        }

        public static async Task<TResult> Run<T1, TResult>(Func<T1, TResult> function, T1 arg1)
        {
            Tuple<Func<T1, TResult>, T1> tuple = new Tuple<Func<T1, TResult>, T1>(function, arg1);

            return await UiTaskFactory.StartNew(obj => ((Tuple<Func<T1, TResult>, T1>)obj).Item1(((Tuple<Func<T1, TResult>, T1>)obj).Item2), tuple);}


        public static async Task<TResult> Run<T1, T2, TResult>(Func<T1, T2, TResult> function, T1 arg1, T2 arg2)
        {
            Tuple<Func<T1, T2, TResult>, T1, T2> tuple = new Tuple<Func<T1, T2, TResult>, T1, T2>(function, arg1, arg2);

            return await UiTaskFactory.StartNew(obj => ((Tuple<Func<T1, T2, TResult>, T1, T2>)obj).Item1(((Tuple<Func<T1, T2, TResult>, T1, T2>)obj).Item2, ((Tuple<Func<T1, T2, TResult>, T1, T2>)obj).Item3),tuple);
        }

        public static async Task<TResult> RunAsync<T1, T2, TResult>(Func<T1, T2, TResult> function, T1 arg1, T2 arg2)
        {
            Tuple<Func<T1, T2, TResult>, T1, T2> tuple = new Tuple<Func<T1, T2, TResult>, T1, T2>(function, arg1, arg2);

            return await UiTaskFactory.StartNew(obj => ((Tuple<Func<T1, T2, TResult>, T1, T2>)obj).Item1(((Tuple<Func<T1, T2, TResult>, T1, T2>)obj).Item2, ((Tuple<Func<T1, T2, TResult>, T1, T2>)obj).Item3), tuple);
        }
    }
}

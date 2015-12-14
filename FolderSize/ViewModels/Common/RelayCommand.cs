using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FolderSize.ViewModels
{
    [DebuggerStepThrough]
    internal class RelayCommand : ICommand
    {
        private readonly Func<object, bool> _onCanExcute;
        private readonly Action<object> _onExecute;

        public RelayCommand(Action<object> onExecute)
        {
            _onExecute = onExecute;
        }

        public RelayCommand(Action onExecute)
        {
            var action = onExecute;
            _onExecute = (o) => action();
        }
        
        public RelayCommand(Func<Task> onExecute)
        {
            var action = onExecute;
            _onExecute = (o) => action();
        }

        public RelayCommand(Func<object, bool> onCanExcute, Action<object> onExecute)
        {
            _onCanExcute = onCanExcute;
            _onExecute = onExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _onCanExcute == null || _onCanExcute(parameter);
        }

        [DebuggerStepThrough]
        public void Execute(object parameter)
        {
            _onExecute?.Invoke(parameter);
        }

        /// <summary>
        /// Tritt ein, wenn Änderungen auftreten, die sich auf die Ausführung des Befehls auswirken.
        /// </summary>
        public event EventHandler CanExecuteChanged;
    }
}
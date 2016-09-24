using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FolderSize.ViewModels
{
    [DebuggerStepThrough]
    internal sealed class RelayCommand : ICommand
    {
        private readonly Func<object, bool> _onCanExcute;
        private readonly Action<object> _onExecute;
        private bool _lastCanExecute;

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
            Func<Task> action = onExecute;
            _onExecute = o => action();
        }

        public RelayCommand(Func<object, bool> onCanExcute, Action<object> onExecute)
        {
            _onCanExcute = onCanExcute;
            _onExecute = onExecute;
        }

        public bool CanExecute(object parameter)
        {
            var result = _onCanExcute == null || _onCanExcute(parameter);
            if (_lastCanExecute == result)
                return result;
            _lastCanExecute = result;
            CanExecuteChanged?.Invoke(this, new EventArgs());
            return result;
        }

        [DebuggerStepThrough]
        public void Execute(object parameter)
        {
            if (CanExecute((parameter)))
                _onExecute?.Invoke(parameter);
        }

        /// <summary>
        /// Tritt ein, wenn Änderungen auftreten, die sich auf die Ausführung des Befehls auswirken.
        /// </summary>
        public event EventHandler CanExecuteChanged;
    }
}
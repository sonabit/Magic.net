using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace
namespace System
{
    /// <summary>
    /// Base type for viewmodel and classes with standard implementation of <see cref="INotifyPropertyChanged"/>
    /// </summary>
    public class ObjectNotify : object, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
// ReSharper restore CheckNamespace
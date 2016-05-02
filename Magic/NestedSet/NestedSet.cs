using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Magic
{
    public sealed class NestedSet<T> : INotifyPropertyChanged,  IEnumerable<T> where T : class
    {
        private readonly LinkedList<NestedSetItem<T>> _set = new LinkedList<NestedSetItem<T>>();
        
        public T Root
        {
            get
            {
                return _set.First.Value.Value;
            }
            set
            {
                _set.Clear();
                LinkedListNode<NestedSetItem<T>> listNode = NestedSetItem<T>.Create(1, 2, value);
                //    listNode = new LinkedListNode<NestedSetItem<T>>(null);
                //NestedSetItem<T> item = new NestedSetItem<T>(1, 2, value, listNode);
                //listNode.Value = item;
                _set.AddFirst(listNode);
                OnPropertyChanged();
                OnPropertyChanged("RootItem");
            }
        }

        /// <summary>
        /// Gibt die Gesamtanzahl der Element im Set an
        /// </summary>
        public int Lenght
        {
            get { return _set.Count; }
        }

        public NestedSetItem<T> RootItem
        {
            get
            {
                return _set.First.Value;
            }
        }

        #region Implementation of IEnumerable

        /// <summary>
        /// Liefert ein Enumerator über die gesamten Elemente im Set
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _set.Select(i => i.Value).AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

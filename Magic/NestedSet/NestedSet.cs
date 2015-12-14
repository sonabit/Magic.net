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
        private readonly SortedSet<NestedSetItem<T>> _set;

        public NestedSet()
        {
            _set = new SortedSet<NestedSetItem<T>>(NestedSetItem<T>.Comparer);
        }

        public T Root
        {
            get
            {
                var first = _set.FirstOrDefault(i => i.Left == 1);
                return first != null ? first.Value : null;
            }
            set
            {
                _set.Clear();
                _set.Add(new NestedSetItem<T>(1, 2, value, _set));
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
            get { return _set.FirstOrDefault(i => i.Left == 1); }
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Magic
{
    /// <summary>
    /// A Entry of a nested set
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("{Value}")]
    public sealed class NestedSetItem<T> : ICollection<NestedSetItem<T>>, INotifyPropertyChanged, INotifyCollectionChanged
        where T : class
    {
        private readonly SortedSet<NestedSetItem<T>> _set;
        internal static IComparer<NestedSetItem<T>> Comparer = new Comparer<NestedSetItem<T>, long>(item => item.Left);

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        internal NestedSetItem(long left, long right, T value, SortedSet<NestedSetItem<T>> set)
        {
            _set = set;
            Left = left;
            Right = right;
            Value = value;
        }

        internal long Left { get; private set; }

        internal long Right { get; private set; }

        public T Value { get; set; }

        #region Implementation of IEnumerable

        public IEnumerator<NestedSetItem<T>> GetEnumerator()
        {
            var n = new NextLevelEnumerator(this);
            var left = Left;
            var right = Right;
            var t = _set.Where(i => i.Left > left && i.Right < right).AsEnumerable().GetEnumerator();
            return n;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public IEnumerable<NestedSetItem<T>> AsAllChildren()
        {
            var left = Left;
            var right = Right;
            var t = _set.Where(i => i.Left > left && i.Right < right).AsEnumerable();
            return t;
        }

        internal class NextLevelEnumerable : IEnumerable<NestedSetItem<T>>
        {
            private readonly NestedSetItem<T> _rootItem;

            public NextLevelEnumerable(NestedSetItem<T> rootItem)
            {
                _rootItem = rootItem;
            }

            public IEnumerator<NestedSetItem<T>> GetEnumerator()
            {
                //var left = _rootItem.Left;
                //var right = _rootItem.Right;
                var t = _rootItem._set.Where(i => i.Left > _rootItem.Left && i.Right < _rootItem.Right).AsEnumerable().GetEnumerator();
                return t;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        class NextLevelEnumerator : IEnumerator<NestedSetItem<T>>
        {
            private readonly NestedSetItem<T> _rootItem;
            private NestedSetItem<T> _item;


            public NextLevelEnumerator(NestedSetItem<T> rootItem)
            {
                _rootItem = rootItem;
            }

            public void Dispose()
            {
                
            }

            public bool MoveNext()
            {
                var left = _item != null ? _item.Right + 1 : _rootItem.Left + 1;
                _item = _rootItem._set.FirstOrDefault(i => i.Left == left);
                return _item != null;
            }

            public void Reset()
            {
                _item = null;
            }

            public NestedSetItem<T> Current { get { return _item; } }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }
        
        #region NestedSetItemEnumerator

        internal class NestedSetItemEnumerator : IEnumerator<T>
        {
            private readonly NestedSetItem<T> _rootItem;
            private IEnumerator<NestedSetItem<T>> _enumerator;

            internal NestedSetItemEnumerator(NestedSetItem<T> rootItem)
            {
                _rootItem = rootItem;
                _enumerator = new NextLevelEnumerator(rootItem);
                // bedingung erstellen nur elemente des nächsten levels
            }

            #region Implementation of IDisposable

            public void Dispose()
            {
                try
                {
                    _enumerator.Dispose();
                }
                catch { }
                _enumerator = null;
            }

            #endregion

            #region Implementation of IEnumerator

            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            public void Reset()
            {
                _enumerator.Reset();
            }

            /// <summary>
            /// Ruft das Element in der Auflistung an der aktuellen Position des Enumerators ab.
            /// </summary>
            /// <returns>
            /// Das Element in der Auflistung an der aktuellen Position des Enumerators.
            /// </returns>
            public T Current
            {
                get { return _enumerator.Current.Value; }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            #endregion
        }

        #endregion


        #region Implementation of ICollection<NestedSetItem<T>>

        public NestedSetItem<T> Add(T item)
        {
            var right = Right;
            _set.Each(i => 
            {
                if (i.Left > right)
                {
                    i.Left += 2;
                    i.Right += 2;
                    return;
                }
                if (i.Right > right)
                {
                    i.Right += 2;
                }
            });
            Right += 2;
            var result = new NestedSetItem<T>(right, right + 1, item, _set);
            _set.Add(result);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new ArrayList {result}));
            RaisePropertyChanged("Count");
            RaisePropertyChanged("TotalCount");
            return result; 
        }

        void ICollection<NestedSetItem<T>>.Add(NestedSetItem<T> item)
        {
            Add(item.Value);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(NestedSetItem<T> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(NestedSetItem<T>[] array, int arrayIndex)
        {
            var idx = arrayIndex;
            foreach (var item in this)
            {
                if (idx >= array.Length) throw new IndexOutOfRangeException();
                array[idx] = item;
                idx++;
            }
        }

        public bool Remove(NestedSetItem<T> item)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get
            {
                var result = 0;
                var  e = new NextLevelEnumerator(this);
                while (e.MoveNext())
                {
                    result++;
                }
                return result;
            }
        }

        public int TotalCount
        {
            get
            {
                return (int)((Right - Left - 1) / 2);
            }
        }

        public bool IsReadOnly { get { return false; } }

        #endregion

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var handler = CollectionChanged;
            if (handler != null) handler(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public   abstract class NestedSetItem
{
    public abstract  object Value { get; protected set; }
}
}
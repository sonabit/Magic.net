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
    public abstract class NestedSetItem
    {
        public abstract object Value { get; protected set; }
    }

    /// <summary>
    ///     A Entry of a nested set
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("{Value}")]
    public sealed class NestedSetItem<T> : ICollection<NestedSetItem<T>>, IList, ICollection, INotifyCollectionChanged,
        INotifyPropertyChanged
        where T : class
    {
        internal static IComparer<NestedSetItem<T>> Comparer = new Comparer<NestedSetItem<T>, long>(item => item.Left);
        private readonly SortedSet<NestedSetItem<T>> _set;

        internal NestedSetItem(long left, long right, T value, SortedSet<NestedSetItem<T>> set)
        {
            _set = set;
            Left = left;
            Right = right;
            Value = value;
            IsFixedSize = false;
        }

        public long Left { get; private set; }

        public long Right { get; private set; }

        public T Value { get; set; }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<NestedSetItem<T>> AsAllChildren()
        {
            var left = Left;
            var right = Right;
            var t = _set.Where(i => i.Left > left && i.Right < right).AsEnumerable();
            return t;
        }

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var handler = CollectionChanged;
            if (handler != null) handler(this, e);
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
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
                var t =
                    _rootItem._set.Where(i => i.Left > _rootItem.Left && i.Right < _rootItem.Right)
                        .AsEnumerable()
                        .GetEnumerator();
                return t;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class NextLevelEnumerator : IEnumerator<NestedSetItem<T>>
        {
            private readonly NestedSetItem<T> _rootItem;


            public NextLevelEnumerator(NestedSetItem<T> rootItem)
            {
                _rootItem = rootItem;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                var left = Current != null ? Current.Right + 1 : _rootItem.Left + 1;
                Current = _rootItem._set.FirstOrDefault(i => i.Left == left);
                return Current != null;
            }

            public void Reset()
            {
                Current = null;
            }

            public NestedSetItem<T> Current { get; private set; }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }

        #region NestedSetItemEnumerator

        internal class NestedSetItemEnumerator : IEnumerator<T>
        {
            //private readonly NestedSetItem<T> _rootItem;
            private IEnumerator<NestedSetItem<T>> _enumerator;

            internal NestedSetItemEnumerator(NestedSetItem<T> rootItem)
            {
                //_rootItem = rootItem;
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
                catch
                {
                    // ignored
                }
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
            ///     Ruft das Element in der Auflistung an der aktuellen Position des Enumerators ab.
            /// </summary>
            /// <returns>
            ///     Das Element in der Auflistung an der aktuellen Position des Enumerators.
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

        #region Implementation of IEnumerable

        public IEnumerator<NestedSetItem<T>> GetEnumerator()
        {
            var enumerator = new NextLevelEnumerator(this);
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<NestedSetItem<T>>

        public NestedSetItem<T> Add(T item)
        {
            var result = new NestedSetItem<T>(0, 0 + 1, item, _set);
            ((ICollection<NestedSetItem<T>>) this).Add(result);

            return result;
        }

        public NestedSetItem<T> NewItem(T item)
        {
            return new NestedSetItem<T>(0, 0 + 1, item, _set);
        }

        public void Add(NestedSetItem<T> item)
        {
            ((ICollection<NestedSetItem<T>>) this).Add(item);
        }

        void ICollection<NestedSetItem<T>>.Add(NestedSetItem<T> item)
        {
            //lock (SyncRoot)
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
                item.Left = right;
                item.Right = right + 1;

                _set.Add(item);
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                new ArrayList {item}));
            RaisePropertyChanged("Count");
            RaisePropertyChanged("TotalCount");
        }

        int IList.Add(object item)
        {
            if (!(item is NestedSetItem<T>)) throw new InvalidCastException("item");
            var idx = Count;
            ((ICollection<NestedSetItem<T>>) this).Add((NestedSetItem<T>) item);
            return idx;
        }

        public bool Contains(object value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(object value)
        {
            var idx = 0;
            foreach (var item in this)
            {
                if (value == item)
                {
                    return idx;
                }
                idx++;
            }
            return -1;
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public object this[int index]
        {
            get { return this.ElementAt(index); }
            set { throw new NotImplementedException(); }
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

        public void CopyTo(Array array, int index)
        {
            var idx = index;
            foreach (var item in this)
            {
                if (idx >= array.Length) throw new IndexOutOfRangeException();
                array.SetValue(item, idx);
                idx++;
            }
        }

        public int Count
        {
            get
            {
                var result = 0;
                var e = new NextLevelEnumerator(this);
                while (e.MoveNext())
                {
                    result++;
                }
                return result;
            }
        }

        public object SyncRoot
        {
            get { return _set; }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public int TotalCount
        {
            get { return (int) ((Right - Left - 1)/2); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool IsFixedSize { get; private set; }

        #endregion
    }
}
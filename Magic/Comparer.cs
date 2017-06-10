using System;
using System.Collections.Generic;

namespace Magic
{
    public class Comparer<T, TComparable> : IComparer<T> where TComparable : IComparable
    {
        private readonly Func<T, TComparable> _func;

        public Comparer(Func<T, TComparable> func)
        {
            _func = func;
        }

        #region Implementation of IComparer<in T>

        public int Compare(T x, T y)
        {
            TComparable a = _func(x);
            return a.CompareTo(_func(y));
        }

        #endregion
    }
}
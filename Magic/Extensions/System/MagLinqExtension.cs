using System.Collections.Generic;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace System.Linq
{
    public static class MagicLinqExtension
    {
        /// <summary>
        /// Performs the specified action on each element of the source.
        /// </summary>
        /// <typeparam name="TSource">Type of the elements</typeparam>
        /// <param name="source">A sequence of values to perform a action</param>
        /// <param name="action">A action to perform</param>
        [DebuggerStepThrough]
        public static void Each<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }

        /// <summary>
        /// Performs the specified action on each element of the source.
        /// </summary>
        /// <typeparam name="TSource">Type of the elements</typeparam>
        /// <typeparam name="T">Type of the parameter</typeparam>
        /// <param name="source">A sequence of values to perform a action</param>
        /// <param name="action">A action to perform</param>
        /// <param name="param">A parameter for the performed action</param>
        [DebuggerStepThrough]
        public static void Each<TSource,T>(this IEnumerable<TSource> source, Action<TSource,T> action, T param)
        {
            foreach (var item in source)
            {
                action(item, param);
            }
        }

        /// <summary>
        /// Performs the specified action on each element of the source.
        /// </summary>
        /// <typeparam name="TSource">Type of the elements</typeparam>
        /// <param name="source">A sequence of values to perform a action</param>
        /// <param name="action">A action to perform</param>
        /// <returns>An IEnumerable&gt;TSource&lt; whose input elements</returns>
        [DebuggerStepThrough]
        public static IEnumerable<TSource> Process<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            using (var e = source.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    action(e.Current);
                    yield return e.Current;
                }
            }
        }

        /// <summary>
        /// Performs the specified action on each element of the source.
        /// </summary>
        /// <typeparam name="TSource">Type of the elements</typeparam>
        /// <typeparam name="T">Type of the parameter</typeparam>
        /// <param name="source">A sequence of values to perform a action</param>
        /// <param name="action">A action to perform</param>
        /// <param name="param">A parameter for the performed action</param>
        /// <returns>An IEnumerable&gt;TSource&lt; whose input elements</returns>
        [DebuggerStepThrough]
        public static IEnumerable<TSource> Process<TSource, T>(this IEnumerable<TSource> source, Action<TSource, T> action, T param)
        {
            using (var e = source.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    action(e.Current, param);
                    yield return e.Current;
                }
            }
        }
    }
}

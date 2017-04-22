using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace VisualStudio2010HelpDownloaderPlus
{
    /// <summary>
    /// Help class with extension methods.
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Perform action on each element from source.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="source">Elements source.</param>
        /// <param name="action">Action to perform.</param>
        /// <returns>Elements source.</returns>
        public static IEnumerable ForEach<T>(this IEnumerable source, Action<T> action)
        {
            Contract.Requires(null != action);

            var forEach = source as object[] ?? source.Cast<object>().ToArray();
            if (null != action)
                foreach (T x in forEach)
                    action(x);

            return forEach;
        }

        /// <summary>
        /// Perform action on each element from source.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="source">Elements source.</param>
        /// <param name="action">Action to perform.</param>
        /// <returns>Elements source.</returns>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            Contract.Requires(null != action);

            var forEach = source as T[] ?? source.ToArray();
            if (null != action)
                foreach (T x in forEach)
                    action(x);

            return forEach;
        }
    }
}
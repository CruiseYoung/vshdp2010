using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

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

            if (null != action)
                foreach (T x in source)
                    action(x);

            return source;
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

            if (null != action)
                foreach (T x in source)
                    action(x);

            return source;
        }
    }
}
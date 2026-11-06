using System;
using System.Collections.Generic;
using System.Linq;

namespace Asteroids.Common
{
    /// <summary>
    /// Sorts items that each declare their own place in an order. Two items in the same place are an error.
    /// </summary>
    public static class UniqueOrder
    {
        /// <param name="orderOf">Function returning place of the item.</param>
        /// <param name="kind">What an item is, for the error message.</param>
        /// <exception cref="ArgumentNullException"><paramref name="items"/> or <paramref name="orderOf"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="items"/> contains null, or two items share an order.</exception>
        public static T[] Sort<T, TOrder>(IEnumerable<T> items, Func<T, TOrder> orderOf, string kind)
            where T : class
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (orderOf == null) throw new ArgumentNullException(nameof(orderOf));

            var ordered = items.ToArray();
            if (Array.IndexOf(ordered, null) >= 0)
                throw new ArgumentException($"The {kind}s must not contain null.", nameof(items));

            var comparer = Comparer<TOrder>.Default;
            var keys = Array.ConvertAll(ordered, item => orderOf(item));
            Array.Sort(keys, ordered, comparer);

            for (var i = 1; i < ordered.Length; i++)
            {
                if (comparer.Compare(keys[i], keys[i - 1]) != 0) continue;

                throw new ArgumentException(
                    $"{ordered[i - 1].GetType().Name} and {ordered[i].GetType().Name} share the order " +
                    $"{keys[i]}. Each {kind} needs its own {typeof(TOrder).Name} value.",
                    nameof(items));
            }

            return ordered;
        }
    }
}
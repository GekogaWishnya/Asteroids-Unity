using System;
using System.Collections.Generic;
using System.Linq;

namespace Asteroids.Collisions
{
    public static class CollisionResponses
    {
        /// <summary>Sorts the responses by <see cref="ICollisionResponse.Order"/>.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="responses"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="responses"/> contains null, or two responses share an order.
        /// </exception>
        public static ICollisionResponse[] InOrder(IEnumerable<ICollisionResponse> responses)
        {
            if (responses == null) throw new ArgumentNullException(nameof(responses));

            var ordered = responses.ToArray();
            if (Array.IndexOf(ordered, null) >= 0)
                throw new ArgumentException("Responses must not contain null.", nameof(responses));

            Array.Sort(ordered, (left, right) => left.Order.CompareTo(right.Order));
            for (var i = 1; i < ordered.Length; i++)
            {
                if (ordered[i].Order != ordered[i - 1].Order) continue;

                throw new ArgumentException(
                    $"{ordered[i - 1].GetType().Name} and {ordered[i].GetType().Name} share the order " +
                    $"{ordered[i].Order}. Each response needs its own {nameof(CollisionResponseOrder)} value.",
                    nameof(responses));
            }

            return ordered;
        }
    }
}

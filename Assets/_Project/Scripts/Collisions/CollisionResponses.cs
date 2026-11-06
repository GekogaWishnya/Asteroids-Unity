using System;
using System.Collections.Generic;
using Asteroids.Common;

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

            return UniqueOrder.Sort(responses, response => response.Order, "response");
        }
    }
}
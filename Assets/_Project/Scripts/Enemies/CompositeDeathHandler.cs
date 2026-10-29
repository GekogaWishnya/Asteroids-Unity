using System;
using System.Collections.Generic;
using UnityEngine;

namespace Asteroids.Enemies
{
    /// <summary>
    /// Runs several death handlers in the order given. If one throws, the exception propagates and the
    /// handlers after it do not run. With no handlers does nothing.
    /// </summary>
    public sealed class CompositeDeathHandler : IEnemyDeathHandler
    {
        private readonly IEnemyDeathHandler[] _handlers;

        /// <param name="handlers">Copied.</param>
        /// <exception cref="ArgumentNullException"><paramref name="handlers"/> is null.</exception>
        /// <exception cref="ArgumentException">A handler is null.</exception>
        public CompositeDeathHandler(IEnumerable<IEnemyDeathHandler> handlers)
        {
            if (handlers == null) throw new ArgumentNullException(nameof(handlers));

            _handlers = new List<IEnemyDeathHandler>(handlers).ToArray();
            foreach (var handler in _handlers)
            {
                if (handler == null)
                    throw new ArgumentException("A death handler must not be null.", nameof(handlers));
            }
        }

        public void Died(Vector2 position)
        {
            foreach (var handler in _handlers) handler.Died(position);
        }
    }
}

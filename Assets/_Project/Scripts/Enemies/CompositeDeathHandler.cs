using System;
using System.Collections.Generic;
using Asteroids.Common;
using UnityEngine;

namespace Asteroids.Enemies
{
    /// <summary>
    /// Runs several death handlers in the order given. If one throws, the others still run and
    /// the first exception is rethrown afterward.
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
            var failures = new FailureCollector();
            foreach (var handler in _handlers)
            {
                try
                {
                    handler.Died(position);
                }
                catch (Exception e)
                {
                    failures.Record(e);
                }
            }

            failures.ThrowIfFailed();
        }
    }
}

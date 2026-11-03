using System;
using Asteroids.Common;

namespace Asteroids.Core
{
    public static class HealthExtensions
    {
        /// <summary>Subscribes <paramref name="handler"/> to <see cref="IHealth.Depleted"/>.</summary>
        /// <returns>Dispose to unsubscribe. Disposing more than once does nothing.</returns>
        /// <exception cref="ArgumentNullException">An argument is null.</exception>
        public static IDisposable SubscribeDepleted(this IHealth health, Action handler)
        {
            if (health == null) throw new ArgumentNullException(nameof(health));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            health.Depleted += handler;
            return new DisposableAction(() => health.Depleted -= handler);
        }
    }
}

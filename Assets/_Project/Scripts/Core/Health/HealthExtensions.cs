using System;
using Asteroids.Common;

namespace Asteroids.Core
{
    public static class HealthExtensions
    {
        /// <summary>Subscribes <paramref name="handler"/> to <see cref="IHealth.Depleted"/>.</summary>
        /// <returns>Dispose to unsubscribe.</returns>
        /// <exception cref="ArgumentNullException">An argument is null.</exception>
        public static IDisposable SubscribeDepleted(this IHealth health, Action handler)
        {
            if (health == null) throw new ArgumentNullException(nameof(health));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            health.Depleted += handler;
            return new DisposableAction(() => health.Depleted -= handler);
        }

        /// <summary>
        /// Calls <paramref name="onDropped"/> after each drop of <see cref="IHealth.Current"/>.
        /// It runs before <see cref="IHealth.Depleted"/> is raised.
        /// </summary>
        /// <returns>Dispose to unsubscribe.</returns>
        /// <exception cref="ArgumentNullException">An argument is null.</exception>
        public static IDisposable SubscribeDropped(this IHealth health, Action<bool> onDropped)
        {
            if (health == null) throw new ArgumentNullException(nameof(health));
            if (onDropped == null) throw new ArgumentNullException(nameof(onDropped));

            var tracker = new DropTracker(health, onDropped);
            return health.Current.Subscribe(tracker.OnChanged);
        }

        private sealed class DropTracker
        {
            private readonly IHealth _health;
            private readonly Action<bool> _onDropped;
            private bool _hasPrevious;
            private int _previous;

            public DropTracker(IHealth health, Action<bool> onDropped)
            {
                _health = health;
                _onDropped = onDropped;
            }

            public void OnChanged(int value)
            {
                var dropped = _hasPrevious && value < _previous;
                _previous = value;
                _hasPrevious = true;

                if (dropped) _onDropped(_health.IsDead);
            }
        }
    }
}
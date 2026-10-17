using System;
using System.Collections.Generic;
using Asteroids.Core;

namespace Asteroids.Player.Tests
{
    internal sealed class FakeDamageRegistry : IDamageRegistry
    {
        private readonly List<Registration<IDamageable>> _damageables = new();
        private readonly List<Registration<IDamageSource>> _sources = new();

        public IReadOnlyList<Registration<IDamageable>> Damageables => _damageables;
        public IReadOnlyList<Registration<IDamageSource>> Sources => _sources;

        public int LiveDamageables
        {
            get
            {
                var count = 0;
                foreach (var registration in _damageables)
                    if (!registration.IsDisposed) count++;
                return count;
            }
        }

        public IDisposable RegisterDamageable(IEntity entity, IDamageable damageable)
        {
            var registration = new Registration<IDamageable>(entity, damageable);
            _damageables.Add(registration);
            return registration;
        }

        public IDisposable RegisterSource(IEntity entity, IDamageSource source)
        {
            var registration = new Registration<IDamageSource>(entity, source);
            _sources.Add(registration);
            return registration;
        }

        public sealed class Registration<T> : IDisposable
        {
            public Registration(IEntity entity, T item)
            {
                Entity = entity;
                Item = item;
            }

            public IEntity Entity { get; }
            public T Item { get; }
            public bool IsDisposed { get; private set; }

            public void Dispose() => IsDisposed = true;
        }
    }
}

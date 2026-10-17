using System;
using System.Collections.Generic;
using Asteroids.Core;

namespace Asteroids.Damage
{
    /// <summary>
    /// Maps entities to something that can be hurt, something that hurts or both.
    /// </summary>
    public sealed class DamageRegistry : IDamageRegistry, IDamageLookup
    {
        private readonly Dictionary<IEntity, IDamageable> _damageables = new();
        private readonly Dictionary<IEntity, IDamageSource> _sources = new();

        public IDisposable RegisterDamageable(IEntity entity, IDamageable damageable) =>
            Register(_damageables, entity, damageable, nameof(damageable));

        public IDisposable RegisterSource(IEntity entity, IDamageSource source) =>
            Register(_sources, entity, source, nameof(source));

        public bool TryGetDamageable(IEntity entity, out IDamageable damageable) =>
            _damageables.TryGetValue(entity, out damageable);

        public bool TryGetSource(IEntity entity, out IDamageSource source) =>
            _sources.TryGetValue(entity, out source);

        private static IDisposable Register<T>(
            Dictionary<IEntity, T> map, IEntity entity, T item, string itemName) where T : class
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (item == null) throw new ArgumentNullException(itemName);

            if (!map.TryAdd(entity, item))
                throw new InvalidOperationException(
                    $"The entity is already registered as {typeof(T).Name}.");

            return new Registration<T>(map, entity, item);
        }

        /// <summary>Removes its own entry on <see cref="Dispose"/>.</summary>
        private sealed class Registration<T> : IDisposable where T : class
        {
            private readonly Dictionary<IEntity, T> _map;
            private readonly IEntity _entity;
            private readonly T _item;

            public Registration(Dictionary<IEntity, T> map, IEntity entity, T item)
            {
                _map = map;
                _entity = entity;
                _item = item;
            }

            public void Dispose()
            {
                if (_map.TryGetValue(_entity, out var current) && ReferenceEquals(current, _item))
                    _map.Remove(_entity);
            }
        }
    }
}

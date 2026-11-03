using System;
using Asteroids.Common;
using Asteroids.Core;

namespace Asteroids.Damage
{
    /// <summary>
    /// Maps entities to something that can be hurt, something that hurts or both.
    /// </summary>
    public sealed class DamageRegistry : IDamageRegistry, IDamageLookup
    {
        private readonly RegistrationMap<IEntity, IDamageable> _damageables = new(nameof(IDamageable));
        private readonly RegistrationMap<IEntity, IDamageSource> _sources = new(nameof(IDamageSource));

        public IDisposable RegisterDamageable(IEntity entity, IDamageable damageable) =>
            _damageables.Add(entity, damageable, nameof(entity), nameof(damageable));

        public IDisposable RegisterSource(IEntity entity, IDamageSource source) =>
            _sources.Add(entity, source, nameof(entity), nameof(source));

        public bool TryGetDamageable(IEntity entity, out IDamageable damageable) =>
            _damageables.TryGet(entity, out damageable);

        public bool TryGetSource(IEntity entity, out IDamageSource source) =>
            _sources.TryGet(entity, out source);
    }
}

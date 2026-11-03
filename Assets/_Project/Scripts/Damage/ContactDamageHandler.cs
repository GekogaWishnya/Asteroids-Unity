using System;
using Asteroids.Collisions;
using Asteroids.Core;

namespace Asteroids.Damage
{
    /// <summary>Damages entities that touch.</summary>
    public sealed class ContactDamageHandler : ICollisionResponse
    {
        private readonly IDamageLookup _lookup;

        /// <exception cref="ArgumentNullException"><paramref name="lookup"/> is null.</exception>
        public ContactDamageHandler(IDamageLookup lookup) =>
            _lookup = lookup ?? throw new ArgumentNullException(nameof(lookup));

        public CollisionResponseOrder Order => CollisionResponseOrder.Damage;

        public void Entered(ICollider first, ICollider second, in Contact contact)
        {
            if (first?.Owner is not IEntity firstEntity) return;
            if (second?.Owner is not IEntity secondEntity) return;
            if (ReferenceEquals(firstEntity, secondEntity)) return;

            var firstHit = Find(victim: firstEntity, attacker: secondEntity);
            var secondHit = Find(victim: secondEntity, attacker: firstEntity);

            try
            {
                firstHit.Apply();
            }
            finally
            {
                secondHit.Apply();
            }
        }

        public void Stayed(ICollider first, ICollider second, in Contact contact) { }

        public void Exited(ICollider first, ICollider second) { }

        private Hit Find(IEntity victim, IEntity attacker)
        {
            if (!_lookup.TryGetDamageable(victim, out var damageable)) return default;
            if (!_lookup.TryGetSource(attacker, out var source)) return default;
            if (source is ISingleUseDamageSource { IsSpent: true }) return default;

            return new Hit(damageable, source.Damage, source as ISingleUseDamageSource);
        }

        private readonly struct Hit
        {
            private readonly IDamageable _target;
            private readonly int _damage;
            private readonly ISingleUseDamageSource _singleUse;

            public Hit(IDamageable target, int damage, ISingleUseDamageSource singleUse)
            {
                _target = target;
                _damage = damage;
                _singleUse = singleUse;
            }

            public void Apply()
            {
                if (_target == null || _damage <= 0) return;

                _singleUse?.Spend();
                _target.TakeDamage(_damage);
            }
        }
    }
}
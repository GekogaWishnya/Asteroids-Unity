using System;
using Asteroids.Collisions;
using Asteroids.Core;

namespace Asteroids.Damage
{
    /// <summary>
    /// Damages other entities on collision. Owner of the <see cref="CollisionEntered"/> must be
    /// registered as damageable and the other entity partaking in the collision must be registered as source.
    /// Otherwise, does nothing after receiving the signal.
    /// </summary>
    public sealed class ContactDamageHandler
    {
        private readonly IDamageLookup _lookup;

        public ContactDamageHandler(IDamageLookup lookup) =>
            _lookup = lookup ?? throw new ArgumentNullException(nameof(lookup));

        public void OnCollisionEntered(CollisionEntered signal)
        {
            if (signal.Self.Owner is not IEntity self) return;
            if (signal.Other.Owner is not IEntity other) return;
            if (!_lookup.TryGetDamageable(self, out var damageable)) return;
            if (!_lookup.TryGetSource(other, out var source)) return;

            var damage = source.Damage;
            if (damage <= 0) return;

            damageable.TakeDamage(damage);
        }
    }
}

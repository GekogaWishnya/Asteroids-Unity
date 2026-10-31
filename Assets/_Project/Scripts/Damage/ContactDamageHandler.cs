using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Asteroids.Collisions;
using Asteroids.Core;

namespace Asteroids.Damage
{
    /// <summary>Damages entities that touch.</summary>
    public sealed class ContactDamageHandler
    {
        private readonly IDamageLookup _lookup;
        private readonly HashSet<ColliderPair> _resolved = new();

        /// <exception cref="ArgumentNullException"><paramref name="lookup"/> is null.</exception>
        public ContactDamageHandler(IDamageLookup lookup) =>
            _lookup = lookup ?? throw new ArgumentNullException(nameof(lookup));

        public void OnCollisionEntered(CollisionEntered signal)
        {
            if (!_resolved.Add(new ColliderPair(signal.Self, signal.Other))) return;
            if (signal.Self.Owner is not IEntity self) return;
            if (signal.Other.Owner is not IEntity other) return;
            
            var selfHit = Find(victim: self, attacker: other);
            var otherHit = Find(victim: other, attacker: self);

            try
            {
                selfHit.Apply();
            }
            finally
            {
                otherHit.Apply();
            }
        }

        public void OnCollisionExited(CollisionExited signal) =>
            _resolved.Remove(new ColliderPair(signal.Self, signal.Other));

        private Hit Find(IEntity victim, IEntity attacker)
        {
            if (!_lookup.TryGetDamageable(victim, out var damageable)) return default;
            if (!_lookup.TryGetSource(attacker, out var source)) return default;

            return new Hit(damageable, source.Damage);
        }

        private readonly struct Hit
        {
            private readonly IDamageable _target;
            private readonly int _damage;

            public Hit(IDamageable target, int damage)
            {
                _target = target;
                _damage = damage;
            }

            public void Apply()
            {
                if (_target != null && _damage > 0) _target.TakeDamage(_damage);
            }
        }
        
        private readonly struct ColliderPair : IEquatable<ColliderPair>
        {
            private readonly ICollider _first;
            private readonly ICollider _second;

            public ColliderPair(ICollider first, ICollider second)
            {
                _first = first;
                _second = second;
            }

            public bool Equals(ColliderPair other) =>
                (ReferenceEquals(_first, other._first) && ReferenceEquals(_second, other._second)) ||
                (ReferenceEquals(_first, other._second) && ReferenceEquals(_second, other._first));

            public override bool Equals(object obj) => obj is ColliderPair other && Equals(other);

            public override int GetHashCode() =>
                RuntimeHelpers.GetHashCode(_first) ^ RuntimeHelpers.GetHashCode(_second);
        }
    }
}

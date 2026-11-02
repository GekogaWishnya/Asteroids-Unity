using System;
using System.Collections.Generic;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Damage;

namespace Asteroids.Weapons.Laser
{
    /// <summary>The targets inside one beam, each with a timer of its own.</summary>
    public sealed class LaserHitTable
    {
        private readonly ILaserWeaponConfig _config;
        private readonly IDamageLookup _damage;
        private readonly Dictionary<IEntity, Target> _targets = new();
        private readonly List<IEntity> _snapshot = new();

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public LaserHitTable(ILaserWeaponConfig config, IDamageLookup damage)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _damage = damage ?? throw new ArgumentNullException(nameof(damage));
        }

        public int Count => _targets.Count;

        public bool Contains(IEntity target) => target != null && _targets.ContainsKey(target);

        /// <summary>One more of the target's colliders touches the beam.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> is null.</exception>
        public void Enter(IEntity target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            if (_targets.TryGetValue(target, out var existing)) existing.Contacts++;
            else _targets.Add(target, new Target());
        }

        /// <summary>One of the target's colliders stopped touching the beam.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> is null.</exception>
        public void Exit(IEntity target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (!_targets.TryGetValue(target, out var existing)) return;

            if (--existing.Contacts <= 0) _targets.Remove(target);
        }

        /// <summary>Forgets every target.</summary>
        public void Clear() => _targets.Clear();

        /// <summary>Counts the timers down and hits every target whose timer ran out.</summary>
        /// <exception cref="InvalidOperationException">The config's hit interval is not a positive number.</exception>
        public void Tick(float deltaTime)
        {
            var interval = _config.HitInterval;
            if (!float.IsFinite(interval) || interval <= 0f)
                throw new InvalidOperationException("Hit interval must be a positive number of seconds.");

            _snapshot.Clear();
            _snapshot.AddRange(_targets.Keys);

            try
            {
                foreach (var entity in _snapshot)
                {
                    if (!_targets.TryGetValue(entity, out var target)) continue;

                    if (target.Remaining > 0f) target.Remaining -= deltaTime;
                    if (target.Remaining > 0f) continue;

                    // Carry the overshoot, but never more than a whole interval: at most one hit per tick.
                    target.Remaining = Math.Max(target.Remaining + interval, 0f);
                    Hit(entity);
                }
            }
            finally
            {
                _snapshot.Clear();
            }
        }

        private void Hit(IEntity entity)
        {
            var damage = _config.HitDamage;
            if (damage > 0 && _damage.TryGetDamageable(entity, out var damageable)) damageable.TakeDamage(damage);
        }

        private sealed class Target
        {
            public int Contacts = 1;
            public float Remaining;
        }
    }
}

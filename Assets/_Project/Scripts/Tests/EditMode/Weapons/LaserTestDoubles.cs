using System;
using System.Collections.Generic;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Damage;
using Asteroids.Weapons.Laser;
using UnityEngine;

namespace Asteroids.Weapons.Tests
{
    internal sealed class TestLaserConfig : ILaserWeaponConfig
    {
        public float MaxEnergy { get; set; } = 100f;
        public float DrainPerSecond { get; set; } = 50f;
        public float RefillPerSecond { get; set; } = 10f;
        public float RefillDelay { get; set; } = 1f;
        public float MinEnergyToFire { get; set; } = 20f;
        public int HitDamage { get; set; } = 1;
        public float HitInterval { get; set; } = 0.5f;
        public float MuzzleOffset { get; set; } = 0.5f;
    }

    internal sealed class FakeBeamView : IPooledView
    {
        public IEntity Entity { get; } = new TestEntity();
        public bool IsShown { get; private set; }
        public bool IsDisposed { get; private set; }
        public int ShowCount { get; private set; }
        public int HideCount { get; private set; }
        public bool FailOnShow { get; set; }

        /// <summary>Called on hide, while the beam is still marked as shown, as a collider leaving play would.</summary>
        public Action OnHide { get; set; }

        public void Show(Vector2 position, float angle)
        {
            if (FailOnShow) throw new InvalidOperationException("show failed");

            Entity.Pose.Position = position;
            Entity.Pose.Angle = angle;
            IsShown = true;
            ShowCount++;
        }

        public void Hide()
        {
            HideCount++;
            OnHide?.Invoke();
            IsShown = false;
        }

        /// <summary>Collider registrations held right now.</summary>
        public int LiveColliders { get; private set; }
        public int ColliderRegistrations { get; private set; }
        public bool FailOnRegisterColliders { get; set; }

        public IDisposable RegisterColliders()
        {
            if (FailOnRegisterColliders) throw new InvalidOperationException("colliders failed");

            ColliderRegistrations++;
            LiveColliders++;
            var released = false;
            return new Asteroids.Common.DisposableAction(() =>
            {
                if (released) return;
                released = true;
                LiveColliders--;
            });
        }

        public void Dispose() => IsDisposed = true;
    }

    internal sealed class RecordingDamageable : IDamageable
    {
        public List<int> Hits { get; } = new();

        /// <summary>Runs after each hit is recorded.</summary>
        public Action OnHit { get; set; }

        public int Total
        {
            get
            {
                var total = 0;
                foreach (var hit in Hits) total += hit;
                return total;
            }
        }

        public void TakeDamage(int amount)
        {
            Hits.Add(amount);
            OnHit?.Invoke();
        }
    }

    internal sealed class FakeDamageLookup : IDamageLookup
    {
        private readonly Dictionary<IEntity, IDamageable> _damageables = new();

        public void Add(IEntity entity, IDamageable damageable) => _damageables.Add(entity, damageable);

        public bool TryGetDamageable(IEntity entity, out IDamageable damageable) =>
            _damageables.TryGetValue(entity, out damageable);

        public bool TryGetSource(IEntity entity, out IDamageSource source)
        {
            source = null;
            return false;
        }
    }

    internal sealed class RecordingBeamContacts : ILaserBeamContacts
    {
        public List<IEntity> EnteredTargets { get; } = new();
        public List<IEntity> ExitedTargets { get; } = new();

        public void Entered(IEntity target) => EnteredTargets.Add(target);
        public void Exited(IEntity target) => ExitedTargets.Add(target);
    }
}

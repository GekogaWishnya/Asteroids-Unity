using System;
using System.Collections.Generic;
using Asteroids.Collisions;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Physics;
using Asteroids.Weapons.Projectiles;
using UnityEngine;

namespace Asteroids.Weapons.Tests
{
    internal sealed class TestEntity : IEntity
    {
        public IPose Pose { get; } = new BodyPose();
    }

    internal sealed class TestWeaponConfig : IProjectileWeaponConfig
    {
        public float FireInterval { get; set; } = 0.25f;
        public float MuzzleOffset { get; set; } = 0.5f;
        public float ProjectileSpeed { get; set; } = 10f;
        public float ProjectileLifetime { get; set; } = 1f;
        public int ProjectileDamage { get; set; } = 2;
        public int PoolPrewarm { get; set; }
        public int PoolMaxIdle { get; set; } = 16;
    }

    internal sealed class TestTrigger : IFireInput
    {
        public bool IsFiring { get; set; }
    }

    internal sealed class FakeProjectileView : IProjectileView
    {
        public IEntity Entity { get; } = new TestEntity();
        public bool IsShown { get; private set; }
        public bool IsDisposed { get; private set; }
        public int ShowCount { get; private set; }
        public bool FailOnShow { get; set; }

        public void Show(Vector2 position, float angle)
        {
            if (FailOnShow) throw new InvalidOperationException("show failed");

            Entity.Pose.Position = position;
            Entity.Pose.Angle = angle;
            IsShown = true;
            ShowCount++;
        }

        public void Hide() => IsShown = false;

        public void Dispose() => IsDisposed = true;
    }

    internal sealed class FakeProjectileViewFactory : IProjectileViewFactory
    {
        public List<FakeProjectileView> Views { get; } = new();

        public IProjectileView Create()
        {
            var view = new FakeProjectileView();
            Views.Add(view);
            return view;
        }

        public int ShownCount
        {
            get
            {
                var count = 0;
                foreach (var view in Views)
                    if (view.IsShown) count++;
                return count;
            }
        }
    }

    /// <summary>Records registrations and ticks the live ones phase by phase, like the real loop.</summary>
    internal sealed class FakeSimulationLoop : ISimulationLoop
    {
        private readonly List<Entry> _entries = new();

        public bool IsPaused { get; set; }

        public int LiveCount(SimulationPhase phase)
        {
            var count = 0;
            foreach (var entry in _entries)
                if (entry.Phase == phase && !entry.IsDisposed) count++;
            return count;
        }

        public IDisposable Register(SimulationPhase phase, ISimulationStep step)
        {
            var entry = new Entry(phase, step);
            _entries.Add(entry);
            return entry;
        }

        /// <summary>Steps registered during the tick start on the next one, as in the real loop.</summary>
        public void Tick(float deltaTime)
        {
            var snapshot = _entries.ToArray();
            foreach (SimulationPhase phase in Enum.GetValues(typeof(SimulationPhase)))
            {
                foreach (var entry in snapshot)
                    if (entry.Phase == phase && !entry.IsDisposed) entry.Step.Tick(deltaTime);
            }
        }

        private sealed class Entry : IDisposable
        {
            public Entry(SimulationPhase phase, ISimulationStep step)
            {
                Phase = phase;
                Step = step;
            }

            public SimulationPhase Phase { get; }
            public ISimulationStep Step { get; }
            public bool IsDisposed { get; private set; }

            public void Dispose() => IsDisposed = true;
        }
    }

    internal sealed class FakeDamageRegistry : IDamageRegistry
    {
        private readonly Dictionary<IEntity, IDamageSource> _sources = new();

        public IDisposable RegisterDamageable(IEntity entity, IDamageable damageable) =>
            throw new NotSupportedException("Projectiles are never damageable.");

        public IDisposable RegisterSource(IEntity entity, IDamageSource source)
        {
            _sources.Add(entity, source);
            return new Removal(() => _sources.Remove(entity));
        }

        public bool TryGetSource(IEntity entity, out IDamageSource source) => _sources.TryGetValue(entity, out source);

        private sealed class Removal : IDisposable
        {
            private Action _remove;

            public Removal(Action remove) => _remove = remove;

            public void Dispose()
            {
                _remove?.Invoke();
                _remove = null;
            }
        }
    }

    internal sealed class FakeCollider : ICollider
    {
        public FakeCollider(object owner) => Owner = owner;

        public object Owner { get; }
        public CollisionLayer Layer => CollisionLayer.None;
        public CollisionLayer Mask => CollisionLayer.All;
        public IShape Shape => null;
    }

    internal sealed class RecordingImpact : IProjectileImpact
    {
        public int Count { get; private set; }

        public void Impact() => Count++;
    }
}

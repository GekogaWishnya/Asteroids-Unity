using System;
using System.Collections.Generic;
using Asteroids.Common;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Physics;
using UnityEngine;

namespace Asteroids.Enemies.Tests
{
    internal sealed class TestPose : IPose
    {
        public Vector2 Position { get; set; }
        public float Angle { get; set; }
        public Vector2 Up => Vector2.up;
        public Vector2 Right => Vector2.right;
    }

    internal sealed class TestEntity : IEntity
    {
        public IPose Pose { get; } = new TestPose();
    }

    internal sealed class FakeEnemyView : IEnemyView
    {
        public IEntity Entity { get; } = new TestEntity();
        public bool IsShown { get; private set; }
        public bool IsDisposed { get; private set; }
        public int ShowCount { get; private set; }
        public int HideCount { get; private set; }
        public bool FailOnShow { get; set; }

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
            IsShown = false;
            HideCount++;
        }

        public void Dispose() => IsDisposed = true;
    }

    internal sealed class FakeEnemyViewFactory : IEnemyViewFactory
    {
        public List<FakeEnemyView> Views { get; } = new();
        public List<EnemyTypeId> Types { get; } = new();

        public IEnemyView Create(EnemyTypeId type)
        {
            var view = new FakeEnemyView();
            Views.Add(view);
            Types.Add(type);
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

        public int DisposedCount
        {
            get
            {
                var count = 0;
                foreach (var view in Views)
                    if (view.IsDisposed) count++;
                return count;
            }
        }
    }

    internal sealed class FakeScoreSink : IScoreSink
    {
        public List<int> Awarded { get; } = new();

        public void Add(int points) => Awarded.Add(points);
    }

    internal sealed class RecordingLauncher : IAsteroidLauncher
    {
        public List<(EnemyTypeId Type, Vector2 Position)> Launches { get; } = new();

        public void Launch(EnemyTypeId type, Vector2 position) => Launches.Add((type, position));
    }

    internal sealed class RecordingDeathHandler : IEnemyDeathHandler
    {
        private readonly List<string> _log;

        public RecordingDeathHandler(List<string> log = null) => _log = log;

        public List<Vector2> Deaths { get; } = new();
        public Exception ThrowOnDied { get; set; }

        public void Died(Vector2 position)
        {
            _log?.Add("died");
            Deaths.Add(position);
            if (ThrowOnDied != null) throw ThrowOnDied;
        }
    }

    /// <summary>Returns the given values in order for <see cref="Value"/> and 0.5 once they run out.</summary>
    internal sealed class ScriptedRandom : IRandom
    {
        private readonly Queue<float> _values;

        public ScriptedRandom(params float[] values) => _values = new Queue<float>(values);

        public float Value() => _values.Count > 0 ? _values.Dequeue() : 0.5f;

        public float Range(float min, float max) => min + Value() * (max - min);

        public int Range(int minInclusive, int maxExclusive) =>
            minInclusive + (int)(Value() * (maxExclusive - minInclusive));
    }

    internal sealed class TestWorldBounds : IWorldBounds
    {
        public TestWorldBounds(Rect rect) => Rect = rect;

        public Rect Rect { get; }

        public Vector2 Wrap(Vector2 position) => position;
    }

    /// <summary>Records registrations and can tick the live ones.</summary>
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

        public void Tick(float deltaTime)
        {
            foreach (SimulationPhase phase in Enum.GetValues(typeof(SimulationPhase)))
            {
                foreach (var entry in _entries.ToArray())
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
        public List<Registration<IDamageable>> Damageables { get; } = new();
        public List<Registration<IDamageSource>> Sources { get; } = new();

        public int LiveDamageables => CountLive(Damageables);
        public int LiveSources => CountLive(Sources);

        public IDisposable RegisterDamageable(IEntity entity, IDamageable damageable)
        {
            var registration = new Registration<IDamageable>(entity, damageable);
            Damageables.Add(registration);
            return registration;
        }

        public IDisposable RegisterSource(IEntity entity, IDamageSource source)
        {
            var registration = new Registration<IDamageSource>(entity, source);
            Sources.Add(registration);
            return registration;
        }

        /// <summary>Hurts every damageable that is registered right now, once.</summary>
        public void HitAll(int amount = 1)
        {
            var count = Damageables.Count;
            for (var i = 0; i < count; i++)
                if (!Damageables[i].IsDisposed) Damageables[i].Item.TakeDamage(amount);
        }

        /// <summary>Hurts the damageable registered most recently.</summary>
        public void HitLatest(int amount = 1) => Damageables[Damageables.Count - 1].Item.TakeDamage(amount);

        private static int CountLive<T>(List<Registration<T>> registrations)
        {
            var count = 0;
            foreach (var registration in registrations)
                if (!registration.IsDisposed) count++;
            return count;
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

    internal sealed class FakeKineticBodyRegistry : IKineticBodyRegistry
    {
        public List<Registration> Registrations { get; } = new();

        public int LiveCount
        {
            get
            {
                var count = 0;
                foreach (var registration in Registrations)
                    if (!registration.IsDisposed) count++;
                return count;
            }
        }

        public IDisposable Register(IEntity entity, IKineticBody body)
        {
            var registration = new Registration(entity, body);
            Registrations.Add(registration);
            return registration;
        }

        public sealed class Registration : IDisposable
        {
            public Registration(IEntity entity, IKineticBody body)
            {
                Entity = entity;
                Body = body;
            }

            public IEntity Entity { get; }
            public IKineticBody Body { get; }
            public bool IsDisposed { get; private set; }

            public void Dispose() => IsDisposed = true;
        }
    }

    internal sealed class TestAsteroidType : IAsteroidTypeConfig
    {
        public TestAsteroidType(string id) => Type = new EnemyTypeId(id);

        public EnemyTypeId Type { get; set; }
        public int MaxHealth { get; set; } = 1;
        public int ContactDamage { get; set; } = 1;
        public int ScorePoints { get; set; } = 20;
        public float MinSpeed { get; set; } = 1f;
        public float MaxSpeed { get; set; } = 2f;
        public float MinSpinSpeed { get; set; } = 10f;
        public float MaxSpinSpeed { get; set; } = 30f;
        public EnemyTypeId ChildType { get; set; }
        public int ChildCount { get; set; }
        public int PoolPrewarm { get; set; }
        public int PoolMaxIdle { get; set; } = 8;

        public TestAsteroidType SplitsInto(string childId, int count)
        {
            ChildType = new EnemyTypeId(childId);
            ChildCount = count;
            return this;
        }
    }

    internal sealed class TestAsteroidConfig : IAsteroidConfig
    {
        public TestAsteroidConfig(params IAsteroidTypeConfig[] types) =>
            Types = new List<IAsteroidTypeConfig>(types);

        public IReadOnlyList<IAsteroidTypeConfig> Types { get; }
    }
}

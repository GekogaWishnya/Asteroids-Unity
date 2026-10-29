using System;
using System.Collections.Generic;
using Asteroids.Core;
using Asteroids.Core.IConfigs;

namespace Asteroids.Waves.Tests
{
    /// <summary>Spawns count as alive until <see cref="KillAll"/>, unless told otherwise.</summary>
    internal sealed class FakeEnemyData : IEnemyData
    {
        private readonly HashSet<EnemyTypeId> _known = new();

        public FakeEnemyData(params string[] knownTypes)
        {
            foreach (var type in knownTypes) _known.Add(new EnemyTypeId(type));
        }

        public int AliveCount { get; set; }
        public List<EnemyTypeId> Spawned { get; } = new();
        public bool SpawnAddsToAlive { get; set; } = true;
        public bool FailOnSpawn { get; set; }

        public bool CanSpawn(EnemyTypeId type) => _known.Contains(type);

        public void Spawn(EnemyTypeId type)
        {
            if (FailOnSpawn) throw new InvalidOperationException("spawn failed");
            if (!CanSpawn(type)) throw new InvalidOperationException($"unknown type '{type}'");

            Spawned.Add(type);
            if (SpawnAddsToAlive) AliveCount++;
        }

        public void KillAll() => AliveCount = 0;
    }

    internal sealed class RecordingWaveEvents : IWaveEventSink
    {
        public List<string> Log { get; } = new();

        public void Started(int number, int total) => Log.Add($"started {number}/{total}");

        public void Cleared(int number) => Log.Add($"cleared {number}");

        public void Completed(int total) => Log.Add($"completed {total}");
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

    internal sealed class TestWaveEntry : IWaveEntryConfig
    {
        public EnemyTypeId Type { get; set; } = new("A");
        public int Count { get; set; } = 1;
        public float StartDelay { get; set; }
        public float Interval { get; set; }
    }

    internal sealed class TestWave : IWaveConfig
    {
        public float DelayBeforeStart { get; set; }

        public IReadOnlyList<IWaveEntryConfig> Entries { get; set; } =
            new List<IWaveEntryConfig> { new TestWaveEntry() };
    }

    internal sealed class TestLevel : ILevelConfig
    {
        public IReadOnlyList<IWaveConfig> Waves { get; set; } = new List<IWaveConfig> { new TestWave() };
    }

    internal static class Build
    {
        public static TestWaveEntry Entry(string type = "A", int count = 1, float start = 0f, float interval = 0f) =>
            new() { Type = new EnemyTypeId(type), Count = count, StartDelay = start, Interval = interval };

        public static TestWave Wave(float delay, params TestWaveEntry[] entries) =>
            new() { DelayBeforeStart = delay, Entries = new List<IWaveEntryConfig>(entries) };

        public static TestLevel Level(params TestWave[] waves) =>
            new() { Waves = new List<IWaveConfig>(waves) };
    }
}

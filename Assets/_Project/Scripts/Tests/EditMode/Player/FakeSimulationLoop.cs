using System;
using System.Collections.Generic;
using Asteroids.Core;

namespace Asteroids.Player.Tests
{
    /// <summary>Records registrations and can tick the live ones.</summary>
    internal sealed class FakeSimulationLoop : ISimulationLoop
    {
        private readonly List<Entry> _entries = new();
        public IReadOnlyList<Entry> Entries => _entries;
        
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

        public sealed class Entry : IDisposable
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
}

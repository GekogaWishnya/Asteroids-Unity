using System;
using Asteroids.Common;
using Asteroids.Core;
using Zenject;

namespace Asteroids.Simulation
{
    /// <summary>
    /// Runs one simulation tick per rendered frame with a single clamped delta time, shared by every step.
    /// </summary>
    public sealed class SimulationLoop : ISimulationLoop, ITickable
    {
        /// <summary>Longest time a single tick may simulate in seconds.</summary>
        private const float MaxDeltaTime = 1f / 20f;
        
        private readonly ISimulationTime _time;
        private readonly DeferredList<Registration>[] _phases;
        private bool _isTicking;

        public SimulationLoop(ISimulationTime time)
        {
            _time = time ?? throw new ArgumentNullException(nameof(time));

            var phases = (SimulationPhase[])Enum.GetValues(typeof(SimulationPhase));
            foreach (var phase in phases)
            {
                if ((int)phase < 0 || (int)phase >= phases.Length)
                    throw new InvalidOperationException(
                        $"{nameof(SimulationPhase)} values must be contiguous and start at zero.");
            }

            _phases = new DeferredList<Registration>[phases.Length];
            for (var i = 0; i < _phases.Length; i++)
                _phases[i] = new DeferredList<Registration>();
        }

        public bool IsPaused { get; set; }

        public IDisposable Register(SimulationPhase phase, ISimulationStep step)
        {
            if (step == null) throw new ArgumentNullException(nameof(step));

            var index = (int)phase;
            if ((uint)index >= (uint)_phases.Length)
                throw new ArgumentOutOfRangeException(nameof(phase), phase, "Unknown simulation phase.");

            var list = _phases[index];
            var registration = new Registration(list, step);
            list.Add(registration);
            return registration;
        }

        public void Tick()
        {
            if (IsPaused) return;
            if (_isTicking) throw new InvalidOperationException("A simulation tick is already running.");

            var deltaTime = Math.Min(_time.DeltaTime, MaxDeltaTime);
            if (!(deltaTime > 0f)) return;

            _isTicking = true;
            foreach (var phase in _phases) phase.Lock();
            try
            {
                foreach (var phase in _phases)
                {
                    var registrations = phase.Items;
                    for (var i = 0; i < registrations.Count; i++)
                        registrations[i].Step.Tick(deltaTime);
                }
            }
            finally
            {
                foreach (var phase in _phases) phase.Unlock();
                _isTicking = false;
            }
        }

        private sealed class Registration : IDisposable
        {
            private DeferredList<Registration> _owner;

            public Registration(DeferredList<Registration> owner, ISimulationStep step)
            {
                _owner = owner;
                Step = step;
            }

            public ISimulationStep Step { get; }

            public void Dispose()
            {
                var owner = _owner;
                if (owner == null) return;

                _owner = null;
                owner.Remove(this);
            }
        }
    }
}

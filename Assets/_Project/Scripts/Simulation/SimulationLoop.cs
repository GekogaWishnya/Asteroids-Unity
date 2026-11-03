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

        /// <summary>
        /// Simulated seconds during which further failures of a step that has just been reported are only
        /// counted, so a step that fails on every tick does not flood the log.
        /// </summary>
        public const float FailureReportInterval = 1f;
        
        private readonly ISimulationTime _time;
        private readonly IErrorLog _errors;
        private readonly DeferredList<Registration>[] _phases;
        private bool _isTicking;
        private double _elapsed;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public SimulationLoop(ISimulationTime time, IErrorLog errors)
        {
            _time = time ?? throw new ArgumentNullException(nameof(time));
            _errors = errors ?? throw new ArgumentNullException(nameof(errors));

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
            var registration = new Registration(list, step, phase);
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
            _elapsed += deltaTime;
            foreach (var phase in _phases) phase.Lock();
            try
            {
                foreach (var phase in _phases)
                {
                    var registrations = phase.Items;
                    for (var i = 0; i < registrations.Count; i++)
                        Run(registrations[i], deltaTime);
                }
            }
            finally
            {
                foreach (var phase in _phases) Unlock(phase);
                _isTicking = false;
            }
        }

        /// <summary>Runs one step. A failing step is reported and the tick goes on.</summary>
        private void Run(Registration registration, float deltaTime)
        {
            try
            {
                registration.Step.Tick(deltaTime);
            }
            catch (Exception e)
            {
                registration.Failed(e, _elapsed, _errors);
            }
        }

        private void Unlock(DeferredList<Registration> phase)
        {
            try
            {
                phase.Unlock();
            }
            catch (Exception e)
            {
                _errors.Report(e, "A simulation step failed while being added or removed.");
            }
        }

        private sealed class Registration : IDisposable
        {
            private readonly SimulationPhase _phase;
            private DeferredList<Registration> _owner;
            private double _lastReport = double.NegativeInfinity;
            private int _suppressed;

            public Registration(DeferredList<Registration> owner, ISimulationStep step, SimulationPhase phase)
            {
                _owner = owner;
                Step = step;
                _phase = phase;
            }

            public ISimulationStep Step { get; }

            /// <summary>Reports the failure, unless this step was reported less than an interval ago.</summary>
            public void Failed(Exception failure, double now, IErrorLog errors)
            {
                if (now - _lastReport < FailureReportInterval)
                {
                    _suppressed++;
                    return;
                }

                var context = $"Simulation step {Step.GetType().Name} ({_phase}) failed; the tick went on without it.";
                if (_suppressed > 0)
                    context += $" {_suppressed} more failure(s) of this step were not reported.";

                _lastReport = now;
                _suppressed = 0;
                errors.Report(failure, context);
            }

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

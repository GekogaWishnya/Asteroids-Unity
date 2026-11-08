using System;
using System.Collections.Generic;
using Asteroids.Common;
using Asteroids.Core;

namespace Asteroids.Session
{
    /// <summary>
    /// The facade of the running game. It is the only holder of <see cref="ISimulationControl"/>, it restarts
    /// the game by restarting every <see cref="IRestartable"/> in <see cref="RestartOrder"/> and it owns the
    /// <see cref="RunLifecycle"/>.
    /// </summary>
    public sealed class GameSession : IGameSession, IDisposable
    {
        private readonly ISimulationControl _simulation;
        private readonly RunLifecycle _lifecycle;
        private readonly IRunEventSink _events;
        private readonly IRestartable[] _restartables;
        private readonly IDisposable _stateSubscription;

        /// <param name="restartables">Must include <paramref name="lifecycle"/> or a restart would not end the run.</param>
        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="restartables"/> contains null, two of them share an order, or it does not contain
        /// <paramref name="lifecycle"/>.
        /// </exception>
        public GameSession(ISimulationControl simulation, RunLifecycle lifecycle, IRunEventSink events,
            IEnumerable<IRestartable> restartables)
        {
            _simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
            _lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            if (restartables == null) throw new ArgumentNullException(nameof(restartables));

            _restartables = UniqueOrder.Sort(restartables, restartable => restartable.Order, "restartable");
            if (Array.IndexOf(_restartables, lifecycle) < 0)
                throw new ArgumentException(
                    $"The {nameof(RunLifecycle)} must be one of the restartables.", nameof(restartables));

            _stateSubscription = _lifecycle.State.Subscribe(OnStateChanged);
        }

        public bool IsPaused => _simulation.IsPaused;

        public IObservableValue<RunState> State => _lifecycle.State;

        public void Pause() => _simulation.IsPaused = true;

        public void Resume()
        {
            if (_lifecycle.State.Value.Phase == RunPhase.Ended) return;

            _simulation.IsPaused = false;
        }

        public void Finish(RunOutcome outcome) => _lifecycle.Finish(outcome);

        public void Restart()
        {
            var failures = new FailureCollector();
            foreach (var restartable in _restartables)
            {
                try
                {
                    restartable.Restart();
                }
                catch (Exception e)
                {
                    failures.Record(e);
                }
            }

            Resume();
            failures.ThrowIfFailed();
        }

        public void Dispose() => _stateSubscription.Dispose();

        private void OnStateChanged(RunState state)
        {
            if (state.Phase != RunPhase.Ended) return;

            Pause();
            _events.Ended(state.Result);
        }
    }
}
using System;
using System.Collections.Generic;
using Asteroids.Common;
using Asteroids.Core;

namespace Asteroids.Session
{
    /// <summary>
    /// The facade of the running game. It is the only holder of <see cref="ISimulationControl"/>, and it restarts
    /// the game by restarting every <see cref="IRestartable"/> in <see cref="RestartOrder"/>.
    /// </summary>
    public sealed class GameSession : IGameSession
    {
        private readonly ISimulationControl _simulation;
        private readonly IRestartable[] _restartables;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="restartables"/> contains null, or two of them share an order.
        /// </exception>
        public GameSession(ISimulationControl simulation, IEnumerable<IRestartable> restartables)
        {
            _simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
            if (restartables == null) throw new ArgumentNullException(nameof(restartables));

            _restartables = UniqueOrder.Sort(restartables, restartable => restartable.Order, "restartable");
        }

        public bool IsPaused => _simulation.IsPaused;

        public void Pause() => _simulation.IsPaused = true;

        public void Resume() => _simulation.IsPaused = false;

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
    }
}

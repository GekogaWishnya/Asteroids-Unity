using System;

namespace Asteroids.Core
{
    /// <summary>
    /// The single driver of the gameplay simulation. Steps are executed phase by phase,
    /// and in registration order inside a phase.
    /// </summary>
    public interface ISimulationLoop
    {
        /// <summary>While true, ticks are skipped entirely. Independent of <c>Time.timeScale</c>.</summary>
        bool IsPaused { get; set; }

        /// <summary>
        /// Adds a step to a phase. Dispose the returned handle to remove it.
        /// Registering or disposing during a tick takes effect after that tick finishes.
        /// </summary>
        IDisposable Register(SimulationPhase phase, ISimulationStep step);
    }
}

using Asteroids.Common;

namespace Asteroids.Core
{
    /// <summary>The running game as a whole.</summary>
    public interface IGameSession
    {
        bool IsPaused { get; }

        /// <summary>
        /// <see cref="RunPhase.Playing"/> until <see cref="Finish"/> decides the outcome, then
        /// <see cref="RunPhase.Ending"/> for the end delay of that outcome, then <see cref="RunPhase.Ended"/>
        /// with the simulation paused. <see cref="Restart"/> brings it back to <see cref="RunPhase.Playing"/>.
        /// </summary>
        IObservableValue<RunState> State { get; }

        /// <summary>Stops the simulation until <see cref="Resume"/>. Nothing moves, spawns or counts down.</summary>
        void Pause();

        /// <summary>Runs the simulation again. Does nothing once the run has <see cref="RunPhase.Ended"/>.</summary>
        void Resume();

        /// <summary>Decides the outcome of the run and freezes its result.</summary>
        void Finish(RunOutcome outcome);

        /// <summary>
        /// Starts the game over. Every <see cref="IRestartable"/> is restarted in <see cref="RestartOrder"/>, then
        /// the simulation runs again.
        /// </summary>
        void Restart();
    }
}
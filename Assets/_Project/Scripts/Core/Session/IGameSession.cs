namespace Asteroids.Core
{
    /// <summary>The running game as a whole.</summary>
    public interface IGameSession
    {
        bool IsPaused { get; }

        /// <summary>Stops the simulation until <see cref="Resume"/>. Nothing moves, spawns or counts down.</summary>
        void Pause();

        void Resume();

        /// <summary>
        /// Starts the game over. Every <see cref="IRestartable"/> is restarted in <see cref="RestartOrder"/>, then
        /// the simulation runs again.
        /// </summary>
        void Restart();
    }
}
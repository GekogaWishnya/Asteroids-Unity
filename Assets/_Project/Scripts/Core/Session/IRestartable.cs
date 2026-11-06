namespace Asteroids.Core
{
    /// <summary>
    /// Restartable state of the game. Bind each implementation as <c>Bind&lt;IRestartable&gt;()</c> and
    /// give it its own <see cref="RestartOrder"/> value.
    /// </summary>
    public interface IRestartable
    {
        RestartOrder Order { get; }

        void Restart();
    }
}
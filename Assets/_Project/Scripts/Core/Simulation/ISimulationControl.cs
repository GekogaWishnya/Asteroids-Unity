namespace Asteroids.Core
{
    /// <summary>
    /// The pause switch of the simulation loop. It is bound only for the <see cref="IGameSession"/>.
    /// </summary>
    public interface ISimulationControl
    {
        bool IsPaused { get; set; }
    }
}
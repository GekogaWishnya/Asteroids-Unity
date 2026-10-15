namespace Asteroids.Core
{
    /// <summary>A unit of simulation work executed once per tick by <see cref="ISimulationLoop"/>.</summary>
    public interface ISimulationStep
    {
        /// <param name="deltaTime">Seconds simulated by this tick. Already clamped by the loop.</param>
        void Tick(float deltaTime);
    }
}

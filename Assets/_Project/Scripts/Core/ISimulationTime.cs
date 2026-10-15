namespace Asteroids.Core
{
    /// <summary>Time source of the simulation loop. Lets the loop run against a fake clock in tests.</summary>
    public interface ISimulationTime
    {
        /// <summary>Seconds since the previous frame. Not clamped.</summary>
        float DeltaTime { get; }
    }
}

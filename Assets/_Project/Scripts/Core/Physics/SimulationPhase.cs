namespace Asteroids.Core
{
    /// <summary>
    /// Phases of a single simulation tick. Phases always run in ascending numeric order.
    /// Values must stay continues and start with zero, because the loop uses them as indices.
    /// </summary>
    public enum SimulationPhase
    {
        Input = 0,
        Motion = 1,
        Collision = 2,
        Logic = 3
    }
}
namespace Asteroids.Core
{
    /// <summary>
    /// Identity of a simulated thing.
    /// </summary>
    public interface IEntity
    {
        IPose Pose { get; }
    }
}

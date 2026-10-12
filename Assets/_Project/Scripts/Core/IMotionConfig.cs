namespace Asteroids.Core
{
    public interface IMotionConfig
    {
        float MaxSpeed { get; }
        float LinearDamping { get; }
    }
}

namespace Asteroids.Core.IConfigs
{
    public interface IMotionConfig
    {
        float MaxSpeed { get; }
        float LinearDamping { get; }
    }
}

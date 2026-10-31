namespace Asteroids.Core.IConfigs
{
    public interface IShipMovementConfig : IMotionConfig, IKnockbackConfig
    {
        float Thrust { get; }
        float TurnSpeed { get; }
    }
}

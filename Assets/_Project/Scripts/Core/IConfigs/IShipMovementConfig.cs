namespace Asteroids.Core.IConfigs
{
    public interface IShipMovementConfig : IMotionConfig
    {
        float Thrust { get; }
        float TurnSpeed { get; }
    }
}

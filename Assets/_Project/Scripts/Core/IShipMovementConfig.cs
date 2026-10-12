namespace Asteroids.Core
{
    public interface IShipMovementConfig : IMotionConfig
    { float Thrust { get; } float TurnSpeed { get; } }
}

namespace Asteroids.Core
{
    public interface IShipMovementConfig
    { float Thrust { get; } float MaxSpeed { get; } float TurnSpeed { get; } float LinearDamping { get; } }
}

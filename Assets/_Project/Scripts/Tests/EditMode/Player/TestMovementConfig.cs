using Asteroids.Core.IConfigs;

namespace Asteroids.Player.Tests
{
    internal sealed class TestMovementConfig : IShipMovementConfig
    {
        public float Thrust { get; set; } = 10f;
        public float TurnSpeed { get; set; } = 100f;
        public float MaxSpeed { get; set; } = 100f;
        public float LinearDamping { get; set; }
    }
}

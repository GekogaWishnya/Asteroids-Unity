using Asteroids.Core;

namespace Asteroids.Player.Tests
{
    internal sealed class TestShipInput : IMovementInput, IFireInput
    {
        public float Turn { get; set; }
        public float Thrust { get; set; }
        public bool IsFiring { get; set; }
    }
}

namespace Asteroids.Player.Tests
{
    internal sealed class FakeProtectionState : IShipProtectionState
    {
        public bool IsProtected { get; set; }
    }
}

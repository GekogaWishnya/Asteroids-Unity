using Asteroids.Core.IConfigs;

namespace Asteroids.Player.Tests
{
    internal sealed class TestHealthConfig : IShipHealthConfig
    {
        public int MaxHealth { get; set; }
    }
}

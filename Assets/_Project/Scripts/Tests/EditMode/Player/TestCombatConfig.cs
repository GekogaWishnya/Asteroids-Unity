using Asteroids.Core.IConfigs;

namespace Asteroids.Player.Tests
{
    internal sealed class TestCombatConfig : IShipRamConfig, IInvulnerabilityConfig, IWaveProtectionConfig, IControlLossConfig
    {
        public int RamDamage { get; set; } = 1;
        public float InvulnerabilitySeconds { get; set; } = 1.5f;
        public CollisionLayer IgnoredLayers { get; set; } = CollisionLayer.Enemy;
        public float WaveStartProtectionSeconds { get; set; } = 2f;
        public float ControlLossSeconds { get; set; } = 3f;
    }
}

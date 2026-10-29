using Asteroids.Core.IConfigs;
using UnityEngine;
using UnityEngine.Serialization;

namespace Asteroids.Data
{
    [CreateAssetMenu(fileName = "ShipHealth", menuName = "Asteroids/Configs/Ship Health")]
    public sealed class ShipHealthConfig : ScriptableObject,
        IShipHealthConfig, IShipRamConfig, IInvulnerabilityConfig, IWaveProtectionConfig
    {
        [Tooltip("Initial and maximum ship health")]
        [FormerlySerializedAs("health")]
        [SerializeField, Min(1)] private int maxHealth = 3;

        [Tooltip("Damage the ship deals to whatever it rams")]
        [SerializeField, Min(1)] private int ramDamage = 1;

        [Tooltip("Seconds after a hit during which the ship takes no further damage. 0 effectively turns it off")]
        [SerializeField, Min(0f)] private float invulnerabilitySeconds = 1.5f;

        [Tooltip("Layers the ship's collider ignores while protected. Do not include Boundary: " +
                 "the ship would stop wrapping around the world.")]
        [SerializeField] private CollisionLayer ignoredLayers = CollisionLayer.Enemy;

        [Tooltip("Seconds the ship is protected when a wave starts. 0 turns it off")]
        [SerializeField, Min(0f)] private float waveStartProtectionSeconds = 2f;

        public int MaxHealth => maxHealth;
        public int RamDamage => ramDamage;
        public float InvulnerabilitySeconds => invulnerabilitySeconds;
        public CollisionLayer IgnoredLayers => ignoredLayers;
        public float WaveStartProtectionSeconds => waveStartProtectionSeconds;

        private void OnValidate()
        {
            if (InvulnerabilityConfigRules.IgnoresBoundary(ignoredLayers))
                Debug.LogWarning(
                    $"{nameof(ShipHealthConfig)} '{name}' ignores the Boundary layer while protected, " +
                    "so the ship is not wrapped around the world during that time.", this);
        }
    }
}

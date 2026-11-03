using Asteroids.Core.IConfigs;
using UnityEngine;
using UnityEngine.Serialization;

namespace Asteroids.Data
{
    [CreateAssetMenu(fileName = "ShipHealth", menuName = "Asteroids/Configs/Ship Health")]
    public sealed class ShipHealthConfig : ScriptableObject,
        IShipHealthConfig, IShipRamConfig, IInvulnerabilityConfig, IWaveProtectionConfig, IControlLossConfig
    {
        [Tooltip("Initial and maximum ship health")]
        [FormerlySerializedAs("health")]
        [SerializeField, Min(1)] private int maxHealth = 3;

        [Tooltip("Damage the ship deals to whatever it rams")]
        [SerializeField, Min(1)] private int ramDamage = 1;

        [Tooltip("Seconds after a hit during which the ship takes no further damage. 0 effectively turns it off")]
        [SerializeField, Min(0f)] private float invulnerabilitySeconds = 3f;

        [Tooltip("Seconds after a hit during which the ship cannot be steered, thrust or fired. 0 effectively turns it off")]
        [SerializeField, Min(0f)] private float controlLossSeconds = 3f;

        [Tooltip("The only layers the ship's colliders still react to while protected. Everything else is ignored")]
        [SerializeField] private CollisionLayer keptLayers = CollisionLayer.None;

        [Tooltip("Seconds the ship is protected when a wave starts. 0 turns it off")]
        [SerializeField, Min(0f)] private float waveStartProtectionSeconds = 2f;

        public int MaxHealth => maxHealth;
        public int RamDamage => ramDamage;
        public float InvulnerabilitySeconds => invulnerabilitySeconds;
        public float ControlLossSeconds => controlLossSeconds;
        public CollisionLayer KeptLayers => keptLayers;
        public float WaveStartProtectionSeconds => waveStartProtectionSeconds;

        private void OnValidate()
        {
            if (controlLossSeconds > invulnerabilitySeconds)
                Debug.LogWarning(
                    $"{nameof(ShipHealthConfig)} '{name}': control loss ({controlLossSeconds} s) outlasts " +
                    $"invulnerability ({invulnerabilitySeconds} s), so the ship can be hit again while the " +
                    "pilot cannot steer away.", this);
        }
    }
}

using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Data
{
    [CreateAssetMenu(fileName = "ShipDurability", menuName = "Asteroids/Configs/ShipDurability")]
    public sealed class ShipDurabilityConfig : ScriptableObject, IShipDurabilityConfig
    {
        [Tooltip("Initial ship health")]
        [SerializeField, Min(1)] private int health = 3;
        
        public int Health => health;
    }
}

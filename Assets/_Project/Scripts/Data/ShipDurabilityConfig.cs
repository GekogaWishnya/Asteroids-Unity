using UnityEngine;

namespace Asteroids.Data
{
    [CreateAssetMenu(fileName = "ShipDurability", menuName = "Asteroids/Configs/ShipDurability")]
    public class ShipDurabilityConfig : ScriptableObject, IShipDurabilityConfig
    {
        [Tooltip("Initial ship health")]
        [SerializeField, Min(0)] private float health = 3;
        
        public float Health => health;
    }
    
    public interface IShipDurabilityConfig
    { float Health { get; } }
}

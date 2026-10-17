using Asteroids.Core.IConfigs;
using UnityEngine;
using UnityEngine.Serialization;

namespace Asteroids.Data
{
    [CreateAssetMenu(fileName = "ShipHealth", menuName = "Asteroids/Configs/ShipHealth")]
    public sealed class ShipHealthConfig : ScriptableObject, IShipHealthConfig
    {
        [Tooltip("Initial and maximum ship health")]
        [FormerlySerializedAs("health")]
        [SerializeField, Min(1)] private int maxHealth = 3;

        public int MaxHealth => maxHealth;
    }
}
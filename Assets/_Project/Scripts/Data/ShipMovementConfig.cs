using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Data
{
    [CreateAssetMenu(fileName = "ShipMovementConfig", menuName = "Asteroids/Configs/ShipMovement")]
    public sealed class ShipMovementConfig : ScriptableObject, IShipMovementConfig
    {
        [Tooltip("Acceleration force applied along the ship's nose (units/s²)")]
        [SerializeField, Min(0f)] private float thrust = 25f;

        [Tooltip("Speed cap (units/s)")]
        [SerializeField, Min(0.1f)] private float maxSpeed = 40f;

        [Tooltip("Rotation speed (degrees/s)")]
        [SerializeField, Min(0f)] private float turnSpeed = 220f;

        [Tooltip("Slow decay of velocity. 0 is pure inertia like the original")]
        [SerializeField, Min(0f)] private float linearDamping = 0f;
        
        public float Thrust => thrust;
        public float MaxSpeed => maxSpeed;
        public float TurnSpeed => turnSpeed;
        public float LinearDamping => linearDamping;

        private void OnValidate()
        {
            var maxPossibleSpeed = Thrust / LinearDamping;
            if (LinearDamping != 0 && maxPossibleSpeed < MaxSpeed)
            {
                Debug.LogWarning($"{name} can't reach MaxSpeed with given configurations. Max possible speed is {maxPossibleSpeed}", this);
            }
        }
    }
}

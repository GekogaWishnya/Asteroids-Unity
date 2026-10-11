using UnityEngine;

namespace Asteroids.Data
{
    [CreateAssetMenu(fileName = "ShipConfig", menuName = "Asteroids/Configs/Ship")]
    public class ShipConfig : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("Acceleration force applied along the ship's nose (units/s²)")]
        [SerializeField, Min(0f)] private float thrust = 25f;

        [Tooltip("Speed cap (units/s)")]
        [SerializeField, Min(0.1f)] private float maxSpeed = 40f;

        [Tooltip("Rotation speed (degrees/s)")]
        [SerializeField, Min(0f)] private float turnSpeed = 220f;

        [Tooltip("Slow decay of velocity. 0 = pure inertia like the original")]
        [SerializeField, Min(0f)] private float linearDamping = 0f;

        [Header("Health")]
        [SerializeField, Min(1)] private int maxHealth = 3;

        [Header("Collision")]
        [Tooltip("Impulse applied to the ship when it hits an enemy")]
        [SerializeField, Min(0f)] private float bounceImpulse = 30f;

        [SerializeField, Min(0f)] private float invulnerabilityDuration = 3f;
        
        public float Thrust => thrust;
        public float MaxSpeed => maxSpeed;
        public float TurnSpeed => turnSpeed;
        public float LinearDamping => linearDamping;
        public int MaxHealth => maxHealth;
        public float BounceImpulse => bounceImpulse;
        public float InvulnerabilityDuration => invulnerabilityDuration;
    }
}

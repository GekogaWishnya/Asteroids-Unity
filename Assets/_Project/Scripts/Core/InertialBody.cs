using UnityEngine;

namespace Asteroids.Core
{
    public class InertialBody : MonoBehaviour, IMotion
    {
        [SerializeField, Min(0.1f)] private float maxSpeed = 40f;
        [Tooltip("Velocity decay per second. 0 = pure inertia")]
        [SerializeField, Min(0f)] private float damping = 0f;

        public Vector2 Velocity { get; set; }
        public float Speed => Velocity.magnitude;
        public float MaxSpeed { get => maxSpeed; set => maxSpeed = Mathf.Max(0.1f, value); }
        public float Damping  { get => damping;  set => damping  = Mathf.Max(0f, value); }

        public void AddVelocity(Vector2 value) => Velocity += value;

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;

            if (damping > 0f)
                Velocity *= Mathf.Exp(-damping * dt);

            Velocity = Vector2.ClampMagnitude(Velocity, maxSpeed);

            Vector3 p = transform.position;
            transform.position = new Vector3(p.x + Velocity.x * dt, p.y + Velocity.y * dt, p.z);
        }
    }
}
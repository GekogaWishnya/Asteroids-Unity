using UnityEngine;

namespace Asteroids.Core
{
    public sealed class InertialBody : MonoBehaviour, IMotionState, IMotionDriver
    {
        private MotionSettings _settings = new(40f, 0f);
        private Vector2 _pendingAcceleration;

        public Vector2 Velocity { get; private set; }
        public float Speed => Velocity.magnitude;

        public void Configure(in MotionSettings settings) => _settings = settings;
        public void Accelerate(Vector2 acceleration) => _pendingAcceleration += acceleration;
        public void ApplyImpulse(Vector2 impulse) => Velocity += impulse;

        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            Velocity += _pendingAcceleration * dt;
            _pendingAcceleration = default;

            if (_settings.Damping > 0f)
                Velocity *= Mathf.Exp(-_settings.Damping * dt);

            Velocity = Vector2.ClampMagnitude(Velocity, _settings.MaxSpeed);
            transform.position += (Vector3)(Velocity * dt);
        }
    }
    
    public interface IMotionState { Vector2 Velocity { get; } float Speed { get; } }

    public interface IMotionDriver
    {
        void Accelerate(Vector2 acceleration); 
        void ApplyImpulse(Vector2 impulse);
    }

    public readonly struct MotionSettings
    {
        public readonly float MaxSpeed, Damping;
        public MotionSettings(float maxSpeed, float damping)
        { MaxSpeed = Mathf.Max(0.1f, maxSpeed); Damping = Mathf.Max(0f, damping); }
    }
}
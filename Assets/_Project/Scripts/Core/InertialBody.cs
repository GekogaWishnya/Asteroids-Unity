using UnityEngine;

namespace Asteroids.Core
{
    public sealed class InertialBody : MonoBehaviour, IMotionState, IMotionDriver
    {
        private IMotionConfig _config;
        private Vector2 _pendingAcceleration;

        public Vector2 Velocity { get; private set; }
        public float Speed => Velocity.magnitude;

        public void Configure(IMotionConfig config) =>
            _config = config ?? throw new System.ArgumentNullException(nameof(config));
        
        public void Accelerate(Vector2 acceleration) => _pendingAcceleration += acceleration;
        public void ApplyImpulse(Vector2 impulse) => Velocity += impulse;

        private void LateUpdate()
        {
            if (_config == null)
            {
                Debug.LogError($"{nameof(InertialBody)} on '{name}' was never configured.", this);
                enabled = false;
                return;
            }
            
            float dt = Time.deltaTime;
            Velocity += _pendingAcceleration * dt;
            _pendingAcceleration = default;

            if (_config.LinearDamping > 0f)
                Velocity *= Mathf.Exp(-_config.LinearDamping * dt);

            Velocity = Vector2.ClampMagnitude(Velocity, _config.MaxSpeed);
            transform.position += (Vector3)(Velocity * dt);
        }
    }
    
    public interface IMotionState
    {Vector2 Velocity { get; } float Speed { get; } }

    public interface IMotionDriver
    {
        void Accelerate(Vector2 acceleration); 
        void ApplyImpulse(Vector2 impulse);
    }

    public interface IMotionConfig
    {
        float MaxSpeed { get; }
        float LinearDamping { get; }
    }
}
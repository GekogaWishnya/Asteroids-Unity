using System;
using UnityEngine;

namespace Asteroids.Core
{
    public sealed class InertialMotion : IMotionState, IMotionDriver
    {
        private readonly IMotionConfig _config;
        private Vector2 _pendingAcceleration;

        public InertialMotion(IMotionConfig config) =>
            _config = config ?? throw new ArgumentNullException(nameof(config));
        
        public Vector2 Velocity { get; private set; }
        public float Speed => Velocity.magnitude;

        public void Accelerate(Vector2 acceleration) => _pendingAcceleration += acceleration;
        public void ApplyImpulse(Vector2 impulse) => Velocity += impulse;

        /// <summary>Advances the simulation.</summary>
        /// <returns>Positional displacement of the object for this step.</returns>
        public Vector2 Step(float deltaTime)
        {
            Velocity += _pendingAcceleration * deltaTime;
            _pendingAcceleration = Vector2.zero;

            if (_config.LinearDamping > 0f)
                Velocity *= Mathf.Exp(-_config.LinearDamping * deltaTime);

            Velocity = Vector2.ClampMagnitude(Velocity, _config.MaxSpeed);
            return Velocity * deltaTime;
        }
    }
}

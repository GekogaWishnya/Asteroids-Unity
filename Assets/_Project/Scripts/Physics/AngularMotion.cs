using System;

namespace Asteroids.Physics
{
    /// <summary>Constant rotation speed.</summary>
    public sealed class AngularMotion
    {
        private float _velocity;

        /// <summary>Degrees per second, counter-clockwise when positive.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The value is not finite.</exception>
        public float Velocity
        {
            get => _velocity;
            set
            {
                if (!float.IsFinite(value))
                    throw new ArgumentOutOfRangeException(nameof(value), "Velocity must be finite.");

                _velocity = value;
            }
        }

        /// <returns>Rotation in degrees for this step.</returns>
        public float Step(float deltaTime) => _velocity * deltaTime;
    }
}

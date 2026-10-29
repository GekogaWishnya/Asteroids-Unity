using UnityEngine;

namespace Asteroids.Enemies
{
    public readonly struct AsteroidLaunch
    {
        public AsteroidLaunch(Vector2 position, float angle, Vector2 velocity, float spinSpeed)
        {
            Position = position;
            Angle = angle;
            Velocity = velocity;
            SpinSpeed = spinSpeed;
        }

        public Vector2 Position { get; }

        /// <summary>Degrees, same convention as <see cref="Asteroids.Core.IPose.Angle"/>.</summary>
        public float Angle { get; }

        /// <summary>Units per second. Independent of <see cref="Angle"/>.</summary>
        public Vector2 Velocity { get; }

        /// <summary>Degrees per second, counter-clockwise when positive.</summary>
        public float SpinSpeed { get; }
    }
}

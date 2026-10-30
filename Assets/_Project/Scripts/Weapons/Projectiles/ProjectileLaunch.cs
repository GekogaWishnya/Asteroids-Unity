using UnityEngine;

namespace Asteroids.Weapons.Projectiles
{
    public readonly struct ProjectileLaunch
    {
        public ProjectileLaunch(Vector2 position, float angle, Vector2 velocity)
        {
            Position = position;
            Angle = angle;
            Velocity = velocity;
        }

        public Vector2 Position { get; }

        /// <summary>Degrees, same convention as <see cref="Asteroids.Core.IPose.Angle"/>.</summary>
        public float Angle { get; }

        /// <summary>Units per second.</summary>
        public Vector2 Velocity { get; }
    }
}

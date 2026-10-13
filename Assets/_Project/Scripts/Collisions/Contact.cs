using UnityEngine;

namespace Asteroids.Collisions
{
    /// <summary>Contact as seen from one collider.</summary>
    /// <param name="normal">unit vector pointing from the other collider towards this one.</param>
    /// <param name="depth">depth of the penetration.</param>
    public readonly struct Contact
    {
        public Contact(Vector2 normal, float depth)
        {
            Normal = normal;
            Depth = depth;
        }

        public Vector2 Normal { get; }
        public float Depth { get; }

        public Contact Inverted => new(-Normal, Depth);
    }
}

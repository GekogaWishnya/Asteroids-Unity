using UnityEngine;

namespace Asteroids.Enemies
{
    /// <summary>A point on the edge of the gameplay area.</summary>
    public readonly struct PerimeterPoint
    {
        public PerimeterPoint(Vector2 origin, Vector2 tangent, Vector2 inward, float length, float along)
        {
            Position = origin + tangent * along;
            Tangent = tangent;
            Inward = inward;
            FromStart = along;
            ToEnd = Mathf.Max(0f, length - along);
        }

        public Vector2 Position { get; }

        /// <summary>Unit vector along the edge, in the direction the perimeter is walked.</summary>
        public Vector2 Tangent { get; }

        /// <summary>Unit vector pointing into the area.</summary>
        public Vector2 Inward { get; }

        /// <summary>Distance from the point to the start corner of its edge.</summary>
        public float FromStart { get; }

        /// <summary>Distance from the point to the end corner of its edge.</summary>
        public float ToEnd { get; }
    }
}

using System;
using UnityEngine;

namespace Asteroids.Collisions
{
    public readonly struct HalfPlane
    {
        private const float MinNormalLength = CollisionConstants.MinLength;

        public HalfPlane(Vector2 inwardNormal, float offset)
        {
            var length = inwardNormal.magnitude;
            if (length < MinNormalLength)
                throw new ArgumentException("Normal must be non-zero.", nameof(inwardNormal));

            InwardNormal = inwardNormal / length;
            Offset = offset / length;
        }

        /// <summary>Unit normal pointing into the allowed region. Zero only for default(HalfPlane).</summary>
        public Vector2 InwardNormal { get; }
        public float Offset { get; }

        /// <summary>Positive inside the allowed region, negative beyond the edge.</summary>
        public float SignedDistance(Vector2 point) => Vector2.Dot(InwardNormal, point) - Offset;
        
        public static HalfPlane FromPoint(Vector2 pointOnEdge, Vector2 inwardNormal) =>
            new(inwardNormal, Vector2.Dot(inwardNormal, pointOnEdge));

        /// <summary>Allowed: x &lt;= value.</summary>
        public static HalfPlane MaxX(float value) => new(Vector2.left, -value);
        /// <summary>Allowed: x &gt;= value.</summary>
        public static HalfPlane MinX(float value) => new(Vector2.right, value);
        /// <summary>Allowed: y &lt;= value.</summary>
        public static HalfPlane MaxY(float value) => new(Vector2.down, -value);
        /// <summary>Allowed: y &gt;= value.</summary>
        public static HalfPlane MinY(float value) => new(Vector2.up, value);
    }
}

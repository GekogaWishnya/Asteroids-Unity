using System;
using UnityEngine;

namespace Asteroids.Collisions
{
    /// <summary>Static invisible wall. Everything beyond the edge of <see cref="Region"/> is penetrating.</summary>
    public sealed class BoundaryShape : IShape
    {
        public BoundaryShape(HalfPlane region)
        {
            if (region.InwardNormal == Vector2.zero)
                throw new ArgumentException("Region is uninitialised (default HalfPlane).", nameof(region));
            Region = region;
        }

        public HalfPlane Region { get; }
    }
}

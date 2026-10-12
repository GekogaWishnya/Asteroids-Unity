using Asteroids.Core;
using UnityEngine;

namespace Asteroids.World
{
    public sealed class WrapBoundaryPolicy : IBoundaryPolicy
    {
        private readonly IWorldBounds _bounds;
        public WrapBoundaryPolicy(IWorldBounds bounds) => _bounds = bounds;
        public Vector2 Apply(Vector2 position, float radius) => _bounds.Wrap(position, radius);
    }
}

using System;
using Asteroids.Collisions;
using Asteroids.Physics;
using Zenject;

namespace Asteroids.World
{
    /// <summary>
    /// Builds the four walls on the edges of <see cref="IWorldBounds.Rect"/>.
    /// Walls report which entities touch the edge and <see cref="WorldWrapHandler"/> decides what to do.
    /// </summary>
    public sealed class WorldBoundary : IInitializable
    {
        private const CollisionLayer WallLayer = CollisionLayer.Boundary;
        private const CollisionLayer WallMask = CollisionLayer.All & ~CollisionLayer.Boundary;

        private readonly IWorldBounds _bounds;
        private readonly IColliderFactory _factory;
        private readonly ICollisionSystem _collisions;

        public WorldBoundary(IWorldBounds bounds, IColliderFactory factory, ICollisionSystem collisions)
        {
            _bounds = bounds ?? throw new ArgumentNullException(nameof(bounds));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _collisions = collisions ?? throw new ArgumentNullException(nameof(collisions));
        }

        public void Initialize()
        {
            var rect = _bounds.Rect;
            AddWall(HalfPlane.MinX(rect.xMin));
            AddWall(HalfPlane.MaxX(rect.xMax));
            AddWall(HalfPlane.MinY(rect.yMin));
            AddWall(HalfPlane.MaxY(rect.yMax));
        }

        // The bounds object is the owner. WorldWrapHandler recognises its own walls by it.
        private void AddWall(HalfPlane region) =>
            _collisions.Register(_factory.CreateBoundary(_bounds, region, WallLayer, WallMask));
    }
}

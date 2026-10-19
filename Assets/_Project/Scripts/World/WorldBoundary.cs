using System;
using Asteroids.Common;
using Asteroids.Collisions;
using Asteroids.Physics;
using Zenject;

namespace Asteroids.World
{
    /// <summary>
    /// Builds the four walls on the edges of <see cref="IWorldBounds.Rect"/>.
    /// Walls report which entities touch the edge and <see cref="WorldWrapHandler"/> decides what to do.
    /// </summary>
    public sealed class WorldBoundary : IInitializable, IDisposable
    {
        private const CollisionLayer WallLayer = CollisionLayer.Boundary;
        private const CollisionLayer WallMask = CollisionLayer.All & ~CollisionLayer.Boundary;

        private readonly IWorldBounds _bounds;
        private readonly ICollisionSystem _collisions;
        private RegistrationScope _walls;

        public WorldBoundary(IWorldBounds bounds, ICollisionSystem collisions)
        {
            _bounds = bounds ?? throw new ArgumentNullException(nameof(bounds));
            _collisions = collisions ?? throw new ArgumentNullException(nameof(collisions));
        }

        public void Initialize()
        {
            var rect = _bounds.Rect;
            _walls = new RegistrationScope(new[]
            {
                Wall(HalfPlane.MinX(rect.xMin)),
                Wall(HalfPlane.MaxX(rect.xMax)),
                Wall(HalfPlane.MinY(rect.yMin)),
                Wall(HalfPlane.MaxY(rect.yMax)),
            });
            _walls.Enable();
        }
        
        public void Dispose() => _walls?.Dispose();

        // The bounds object is the owner. WorldWrapHandler recognises its own walls by it.
        private Func<IDisposable> Wall(HalfPlane region) =>
            () => _collisions.Register(new ShapeCollider(_bounds, WallLayer, WallMask, new BoundaryShape(region)));
    }
}

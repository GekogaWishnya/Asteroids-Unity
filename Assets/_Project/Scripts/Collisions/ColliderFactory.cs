using Zenject;

namespace Asteroids.Collisions
{
    public sealed class ColliderFactory : IColliderFactory
    {
        private readonly SignalBus _signalBus;

        public ColliderFactory(SignalBus signalBus) => _signalBus = signalBus;

        public CircleCollider CreateCircle(object owner, float radius, CollisionLayer layer, CollisionLayer mask) =>
            new(_signalBus, owner, radius, layer, mask);

        public BoundaryCollider CreateBoundary(object owner, HalfPlane region, CollisionLayer layer, CollisionLayer mask) =>
            new(_signalBus, owner, region, layer, mask);
    }
}
namespace Asteroids.Collisions
{
    /// <summary>Lets owners create colliders without knowing about SignalBus. Does not register them.</summary>
    public interface IColliderFactory
    {
        CircleCollider CreateCircle(object owner, float radius, CollisionLayer layer, CollisionLayer mask);
        BoundaryCollider CreateBoundary(object owner, HalfPlane region, CollisionLayer layer, CollisionLayer mask);
    }
}
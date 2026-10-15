using Asteroids.Core;
using UnityEngine;

namespace Asteroids.Collisions
{
    /// <summary>Lets owners create colliders without knowing about SignalBus. Does not register them.</summary>
    public interface IColliderFactory
    {
        /// <param name="owner">What signal handlers will see as <see cref="ICollider.Owner"/>.</param>
        /// <param name="pose">Position source. Read lazily by the collider.</param>
        /// <param name="offset">Center offset in the pose's local space.</param>
        CircleCollider CreateCircle(object owner, IPose pose, Vector2 offset, float radius,
            CollisionLayer layer, CollisionLayer mask);

        BoundaryCollider CreateBoundary(object owner, HalfPlane region, CollisionLayer layer, CollisionLayer mask);
    }
}

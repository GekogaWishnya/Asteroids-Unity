using UnityEngine;

namespace Asteroids.Collisions
{
    /// <summary>
    /// Rules for collider setup. Components use them to warn about configurations that are valid
    /// to serialize but silently misbehave at runtime.
    /// </summary>
    public static class ColliderSetupRules
    {
        /// <summary>
        /// A collider with no layer is never part of any pair, so it interacts with nothing.
        /// </summary>
        public static bool HasNoLayer(CollisionLayer layer) => layer == CollisionLayer.None;

        /// <summary>
        /// An entity that wraps around the world needs a collider whose center offset is smaller than its radius,
        /// otherwise the collider can sit entirely outside the entity's pose position while it crosses an edge.
        /// </summary>
        public static bool OffsetReachesRadius(Vector2 offset, float radius) => offset.magnitude >= radius;
    }
}
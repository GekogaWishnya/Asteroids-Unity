using Asteroids.Core;
using UnityEngine;

namespace Asteroids.Collisions.Components
{
    /// <summary>
    /// Circle collider that follows the <see cref="IEntity"/>.
    /// </summary>
    [AddComponentMenu("Asteroids/Collisions/Circle Collider")]
    public sealed class CircleColliderComponent : ColliderComponent
    {
        [Tooltip("Centre offset in the entity's local space.")]
        [SerializeField] private Vector2 offset;
        [Tooltip("Radius in world space units.")]
        [SerializeField, Min(0f)] private float radius = 1f;

        protected override Color GizmoColor => new(0.2f, 1f, 0.4f);

        protected override ShapeCollider CreateCollider(object owner)
        {
            var entity = RequireEntity(owner);
            return new ShapeCollider(entity, Layer, Mask, new CircleShape(entity.Pose, offset, radius));
        }

        protected override void DrawShape()
        {
            var current = transform;
            Vector2 center = current.position + current.rotation * (Vector3)offset;
            GizmoShapes.Circle(center, radius);
        }
    }
}

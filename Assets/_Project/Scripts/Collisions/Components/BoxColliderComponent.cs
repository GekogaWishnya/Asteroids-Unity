using UnityEngine;

namespace Asteroids.Collisions.Components
{
    [AddComponentMenu("Asteroids/Collisions/Box Collider")]
    public sealed class BoxColliderComponent : ColliderComponent
    {
        [Tooltip("Centre offset in the entity's local space.")]
        [SerializeField] private Vector2 offset;
        [Tooltip("Width and height in world space units.")]
        [SerializeField] private Vector2 size = Vector2.one;

        protected override Color GizmoColor => new(0.3f, 0.75f, 1f);

        protected override ShapeCollider CreateCollider(object owner)
        {
            var entity = RequireEntity(owner);
            return new ShapeCollider(entity, Layer, Mask, new OrientedBoxShape(entity.Pose, offset, size));
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            size = Vector2.Max(size, Vector2.zero);
        }

        protected override void DrawShape()
        {
            var current = transform;
            var rotation = current.rotation;
            Vector2 center = current.position + rotation * offset;
            GizmoShapes.Box(center, rotation * Vector3.right, rotation * Vector3.up, size * 0.5f);
        }
    }
}

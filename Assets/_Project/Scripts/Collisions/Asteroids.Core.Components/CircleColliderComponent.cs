using UnityEngine;

namespace Asteroids.Collisions.Components
{
    [AddComponentMenu("Asteroids/Collisions/Circle Collider")]
    public sealed class CircleColliderComponent : ColliderComponent
    {
        [Tooltip("Centre offset in local space.")]
        [SerializeField] private Vector2 offset;
        [Tooltip("Radius in units. Scaled by the largest axis of the object's scale.")]
        [SerializeField, Min(0f)] private float radius = 1f;

        private CircleCollider _circle;

        protected override Color GizmoColor => new(0.2f, 1f, 0.4f);

        private Vector2 WorldCenter => transform.TransformPoint(offset);

        private float WorldRadius
        {
            get
            {
                var scale = transform.lossyScale;
                return radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
            }
        }

        protected override BaseCollider CreateCollider(IColliderFactory factory, object owner)
        {
            _circle = factory.CreateCircle(owner, WorldRadius, Layer, Mask);
            return _circle;
        }

        protected override void Synchronize()
        {
            _circle.Position = WorldCenter;
            _circle.Radius = WorldRadius;
        }

        protected override void DrawShape() => GizmoShapes.Circle(WorldCenter, WorldRadius);
    }
}
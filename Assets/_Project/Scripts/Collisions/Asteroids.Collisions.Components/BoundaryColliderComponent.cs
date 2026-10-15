using UnityEngine;

namespace Asteroids.Collisions.Components
{
    /// <summary>
    /// Invisible wall. The edge passes through this object's position and <see cref="inwardDirection"/>
    /// points into the allowed area.
    /// The region is captured when the component is enabled. To move this collider, disable and re-enable it.
    /// </summary>
    [AddComponentMenu("Asteroids/Collisions/Boundary Collider")]
    public sealed class BoundaryColliderComponent : ColliderComponent
    {
        [Tooltip("Offset of the central point on the edge of the boundary collider in world space along the inwardDirection.")]
        [SerializeField] private float pointOffset;
        [Tooltip("Direction pointing into the allowed area, in degrees counter-clockwise from +X in local space.")]
        [SerializeField, Range(-180f, 180f)] private float inwardAngle = -90f;
        [Tooltip("Gizmo only: how far the edge line is drawn on each side of the object.")]
        [SerializeField, Min(0.1f)] private float gizmoHalfLength = 10f;

        protected override Color GizmoColor => new(1f, 0.55f, 0.1f);

        private Vector2 WorldInward
        {
            get
            {
                var radians = inwardAngle * Mathf.Deg2Rad;
                var local = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f);
                return transform.TransformDirection(local).normalized;
            }
        }
        private Vector2 OffsetPosition => (Vector2)transform.position + pointOffset * WorldInward;

        protected override BaseCollider CreateCollider(IColliderFactory factory, object owner) =>
            factory.CreateBoundary(owner, 
                HalfPlane.FromPoint(OffsetPosition, WorldInward),
                Layer, Mask);

        protected override void DrawShape() =>
            GizmoShapes.Wall(OffsetPosition, WorldInward, gizmoHalfLength);
    }
}

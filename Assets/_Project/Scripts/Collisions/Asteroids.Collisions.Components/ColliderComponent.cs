using Asteroids.Core;
using UnityEngine;
using Zenject;

namespace Asteroids.Collisions.Components
{
    /// <summary>
    /// Scene adapter over a collider. Decides lifecycle of the collider and nothing else.
    /// The collider's <see cref="ICollider.Owner"/> is the <see cref="IEntity"/> of a sibling
    /// <see cref="IEntityProvider"/> when there is one, and this GameObject otherwise.
    /// Layer and mask are read when the collider is created, so changes at runtime apply after a re-enable.
    /// </summary>
    public abstract class ColliderComponent : MonoBehaviour
    {
        [Tooltip("Layers this collider belongs to.")]
        [SerializeField] private CollisionLayer layer = CollisionLayer.None;
        [Tooltip("Layers this collider reacts to. A pair interacts only if both masks accept the other's layer.")]
        [SerializeField] private CollisionLayer mask = CollisionLayer.All;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = false;

        private IColliderFactory _factory;
        private ICollisionSystem _system;
        private BaseCollider _collider;

        protected CollisionLayer Layer => layer;
        protected CollisionLayer Mask => mask;

        /// <summary>Null while the component is disabled.</summary>
        public ICollider Collider => _collider;

        protected abstract Color GizmoColor { get; }
        protected abstract BaseCollider CreateCollider(IColliderFactory factory, object owner);
        protected abstract void DrawShape();

        [Inject]
        private void Construct(IColliderFactory factory, ICollisionSystem system)
        {
            _factory = factory;
            _system = system;

            if (isActiveAndEnabled) Attach();
        }

        private void OnEnable()
        {
            if (_system != null) Attach();
        }

        private void OnDisable() => Detach();

        private void Attach()
        {
            if (_collider != null) return;

            _collider = CreateCollider(_factory, ResolveOwner());
            _system.Register(_collider);
        }

        private void Detach()
        {
            if (_collider == null) return;

            _system.Unregister(_collider);
            _collider = null;
        }

        private object ResolveOwner() =>
            TryGetComponent<IEntityProvider>(out var provider) ? (object)provider.Entity : gameObject;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!drawGizmos || !enabled) return;
            var color = GizmoColor;
            color.a = 0.35f;
            Gizmos.color = color;
            DrawShape();
        }

        private void OnDrawGizmosSelected()
        {
            if (!enabled) return;
            Gizmos.color = GizmoColor;
            DrawShape();
        }
#endif
    }
}

using System;
using Asteroids.Common;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using UnityEngine;
using Zenject;

namespace Asteroids.Collisions.Components
{
    /// <summary>
    /// Scene adapter over a collider. Decides lifecycle of the collider and nothing else.
    /// The collider's <see cref="ICollider.Owner"/> is the <see cref="IEntity"/> of a sibling
    /// <see cref="IEntityProvider"/> when there is one, and this GameObject otherwise.
    /// Layer and mask are read when the collider is created, so changes at runtime apply after a re-enable.
    /// <see cref="Ignore"/> is the exception. It narrows the live collider's mask and is kept while collider is
    /// disabled. <see cref="Restore"/> restores the original mask.
    /// </summary>
    public abstract class ColliderComponent : MonoBehaviour, ICollisionIgnore, IManagedCollider
    {
        [Tooltip("Layers this collider belongs to.")]
        [SerializeField] private CollisionLayer layer = CollisionLayer.None;
        [Tooltip("Layers this collider reacts to. A pair interacts only if both masks accept the other's layer.")]
        [SerializeField] private CollisionLayer mask = CollisionLayer.All;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = false;

        private ICollisionSystem _system;
        private RegistrationScope _registration;
        private ShapeCollider _collider;
        private CollisionLayer _ignored = CollisionLayer.None;
        private bool _managed;
        private int _generation;

        protected CollisionLayer Layer => layer;

        /// <summary>The mask the collider should have at a given moment.</summary>
        protected CollisionLayer Mask => mask & ~_ignored;

        /// <summary>Null while the component is disabled.</summary>
        public ICollider Collider => _collider;

        protected abstract Color GizmoColor { get; }
        protected abstract ShapeCollider CreateCollider(object owner);
        protected abstract void DrawShape();

        public void Ignore(CollisionLayer layers)
        {
            _ignored = layers;
            if (_collider != null) _collider.Mask = Mask;
        }

        public void Restore() => Ignore(CollisionLayer.None);

        public void TakeOverRegistration()
        {
            _managed = true;
            _registration?.Disable();
        }

        public IDisposable Register()
        {
            if (!_managed)
                throw new InvalidOperationException(
                    $"{GetType().Name} on '{name}' registers itself; call {nameof(TakeOverRegistration)} first.");
            if (_registration == null)
                throw new InvalidOperationException($"{GetType().Name} on '{name}' is not injected yet.");
            if (_registration.IsEnabled)
                throw new InvalidOperationException($"{GetType().Name} on '{name}' is already registered.");

            _registration.Enable();

            // A late dispose of an older handle must not take down a newer registration.
            var generation = ++_generation;
            return new DisposableAction(() =>
            {
                if (generation == _generation) _registration.Disable();
            });
        }

        [Inject]
        private void Construct(ICollisionSystem system)
        {
            _system = system;
            _registration = new RegistrationScope(new Func<IDisposable>[] { RegisterCollider });

            if (isActiveAndEnabled && !_managed) _registration.Enable();
        }

        private void OnEnable()
        {
            if (!_managed) _registration?.Enable();
        }

        private void OnDisable()
        {
            if (!_managed) _registration?.Disable();
        }

        protected virtual void OnValidate()
        {
            if (ColliderSetupRules.HasNoLayer(layer))
                Debug.LogWarning(
                    $"{GetType().Name} on '{name}' has layer None.", this);
        }

        private IDisposable RegisterCollider()
        {
            var col = CreateCollider(ResolveOwner());
            var handle = _system.Register(col);
            _collider = col;

            return new DisposableAction(() =>
            {
                try
                {
                    handle.Dispose();
                }
                finally
                {
                    _collider = null;
                }
            });
        }

        /// <summary>The sibling entity if there is one, otherwise this GameObject.</summary>
        private object ResolveOwner() =>
            TryGetComponent<IEntityProvider>(out var provider) ? provider.Entity : gameObject;

        /// <summary>
        /// The single place that enforces "this collider follows an entity".
        /// </summary>
        /// <exception cref="InvalidOperationException">The owner is not an <see cref="IEntity"/>: no sibling <see cref="IEntityProvider"/>.</exception>
        protected IEntity RequireEntity(object owner) =>
            owner as IEntity
            ?? throw new InvalidOperationException(
                $"{GetType().Name} on '{name}' needs a sibling component implementing " +
                $"{nameof(IEntityProvider)} (add EntityComponent).");

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
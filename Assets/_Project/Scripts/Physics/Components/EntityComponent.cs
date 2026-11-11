using System;
using Asteroids.Core;
using UnityEngine;
using Zenject;

namespace Asteroids.Physics.Components
{
    /// <summary>
    /// Scene adapter that owns the <see cref="Entity"/> of a GameObject.
    /// The pose is generated from the transform once, then the transform follows the pose through
    /// <see cref="IPoseViewRegistry"/> while this component is enabled.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Asteroids/Physics/Entity")]
    public sealed class EntityComponent : MonoBehaviour, IEntityProvider, IPoseView
    {
        private Entity _entity;
        private IPoseViewRegistry _registry;
        private IDisposable _following;

        public IEntity Entity => _entity ??= CreateEntity();

        [Inject]
        private void Construct(IPoseViewRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            if (isActiveAndEnabled) Follow();
        }

        private void Awake() => _entity ??= CreateEntity();

        private void OnEnable() => Follow();

        /// <exception cref="InvalidOperationException">Not injected, so the transform would never move.</exception>
        private void Start()
        {
            if (_registry == null)
                throw new InvalidOperationException(
                    $"{nameof(EntityComponent)} on '{name}' was not injected, so its transform would never follow " +
                    "its pose. Place it in the scene or instantiate it through the container.");
        }

        private void OnDisable()
        {
            _following?.Dispose();
            _following = null;
        }

        void IPoseView.Show(Vector2 position, float angle)
        {
            var current = transform;
            current.SetPositionAndRotation(
                new Vector3(position.x, position.y, current.position.z),
                Quaternion.Euler(0f, 0f, angle));
        }

        private void Follow()
        {
            if (_registry == null || _following != null) return;
            _following = _registry.Register(Entity.Pose, this);
        }

        private Entity CreateEntity()
        {
            var entity = new Entity(new BodyPose());
            var pose = entity.Pose;
            pose.Position = transform.position;
            pose.Angle = transform.eulerAngles.z;
            return entity;
        }
    }
}
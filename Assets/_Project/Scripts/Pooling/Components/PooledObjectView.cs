using System;
using System.Collections.Generic;
using Asteroids.Common;
using Asteroids.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Asteroids.Pooling.Components
{
    public sealed class PooledObjectView : IPooledView
    {
        private readonly Transform _transform;
        private readonly Transform _idleRoot;
        private readonly Transform _activeRoot;
        private readonly bool _ownsRoots;
        private readonly IManagedCollider[] _colliders;

        /// <param name="ownsRoots">Destroy both roots with the object, for a view that has roots of its own.</param>
        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public PooledObjectView(
            Transform transform, IEntity entity, Transform idleRoot, Transform activeRoot, bool ownsRoots = false)
        {
            _transform = transform ? transform : throw new ArgumentNullException(nameof(transform));
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
            _idleRoot = idleRoot ? idleRoot : throw new ArgumentNullException(nameof(idleRoot));
            _activeRoot = activeRoot ? activeRoot : throw new ArgumentNullException(nameof(activeRoot));
            _ownsRoots = ownsRoots;

            _colliders = transform.GetComponentsInChildren<IManagedCollider>(true);
            foreach (var collider in _colliders) collider.TakeOverRegistration();
        }

        public IEntity Entity { get; }

        public void Show(Vector2 position, float angle)
        {
            _transform.SetParent(_activeRoot, false);

            var pose = Entity.Pose;
            pose.Position = position;
            pose.Angle = angle;

            _transform.SetPositionAndRotation(
                new Vector3(position.x, position.y, _transform.position.z),
                Quaternion.Euler(0f, 0f, angle));
        }

        public void Hide()
        {
            if (_transform) _transform.SetParent(_idleRoot, false);
        }

        public IDisposable RegisterColliders()
        {
            var registrations = new List<Func<IDisposable>>(_colliders.Length);
            foreach (var collider in _colliders) registrations.Add(collider.Register);

            var scope = new RegistrationScope(registrations);
            scope.Enable();
            return scope;
        }

        public void Dispose()
        {
            if (_transform) Object.Destroy(_transform.gameObject);
            if (!_ownsRoots) return;

            if (_idleRoot) Object.Destroy(_idleRoot.gameObject);
            if (_activeRoot) Object.Destroy(_activeRoot.gameObject);
        }
    }
}
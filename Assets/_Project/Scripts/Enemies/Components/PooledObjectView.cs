using Asteroids.Core;
using UnityEngine;

namespace Asteroids.Enemies.Components
{
    /// <summary>
    /// A pooled object that is in play while it is under the active root and inactive while it hangs under
    /// the inactive one. The reparenting is what changes its state.
    /// </summary>
    internal sealed class PooledObjectView : IEnemyView
    {
        private readonly Transform _transform;
        private readonly Transform _idleRoot;
        private readonly Transform _activeRoot;

        public PooledObjectView(Transform transform, IEntity entity, Transform idleRoot, Transform activeRoot)
        {
            _transform = transform;
            Entity = entity;
            _idleRoot = idleRoot;
            _activeRoot = activeRoot;
        }

        public IEntity Entity { get; }

        public void Show(Vector2 position, float angle)
        {
            _transform.SetParent(_activeRoot, false);

            var pose = Entity.Pose;
            pose.Position = position;
            pose.Angle = angle;

            // The entity mirrors its pose to the transform in LateUpdate, which may not run for a freshly
            // woken object before the first frame is drawn.
            _transform.SetPositionAndRotation(
                new Vector3(position.x, position.y, _transform.position.z),
                Quaternion.Euler(0f, 0f, angle));
        }

        public void Hide() => _transform.SetParent(_idleRoot, false);

        public void Dispose()
        {
            if (_transform) UnityEngine.Object.Destroy(_transform.gameObject);
        }
    }
}

using Asteroids.Core;
using Asteroids.Weapons.Laser;
using UnityEngine;

namespace Asteroids.Weapons.Components
{
    /// <summary>The beam object of one laser gun.</summary>
    internal sealed class LaserBeamView : ILaserBeamView
    {
        private readonly Transform _transform;
        private readonly Transform _idleRoot;
        private readonly Transform _activeRoot;

        public LaserBeamView(Transform transform, IEntity entity, Transform idleRoot, Transform activeRoot)
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
            
            _transform.SetPositionAndRotation(
                new Vector3(position.x, position.y, _transform.position.z),
                Quaternion.Euler(0f, 0f, angle));
        }

        public void Hide()
        {
            if (_transform) _transform.SetParent(_idleRoot, false);
        }

        public void Dispose()
        {
            if (_transform) Object.Destroy(_transform.gameObject);
            if (_idleRoot) Object.Destroy(_idleRoot.gameObject);
            if (_activeRoot) Object.Destroy(_activeRoot.gameObject);
        }
    }
}

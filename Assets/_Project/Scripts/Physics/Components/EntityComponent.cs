using Asteroids.Core;
using UnityEngine;

namespace Asteroids.Physics.Components
{
    /// <summary>
    /// Scene adapter that owns the <see cref="Entity"/> of a GameObject.
    /// The pose is generated from the transform once, then transform mirrors pose.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Asteroids/Physics/Entity")]
    public sealed class EntityComponent : MonoBehaviour, IEntityProvider
    {
        private readonly Entity _entity = new(new BodyPose());

        public IEntity Entity => _entity;

        private void Awake()
        {
            var pose = _entity.Pose;
            pose.Position = transform.position;
            pose.Angle = transform.eulerAngles.z;
        }

        // LateUpdate always runs after the simulation loop has ticked.
        private void LateUpdate()
        {
            var pose = _entity.Pose;
            var current = transform;
            var position = pose.Position;

            current.SetPositionAndRotation(
                new Vector3(position.x, position.y, current.position.z),
                Quaternion.Euler(0f, 0f, pose.Angle));
        }
    }
}

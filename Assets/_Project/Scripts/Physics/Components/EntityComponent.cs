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
        private Entity _entity;

        public IEntity Entity => _entity ??= CreateEntity();

        private void Awake() => _entity ??= CreateEntity();

        private Entity CreateEntity()
        {
            var entity = new Entity(new BodyPose());
            var pose = entity.Pose;
            pose.Position = transform.position;
            pose.Angle = transform.eulerAngles.z;
            return entity;
        }

        // LateUpdate always runs after the simulation loop has ticked.
        private void LateUpdate()
        {
            var pose = Entity.Pose;
            var current = transform;
            var position = pose.Position;

            current.SetPositionAndRotation(
                new Vector3(position.x, position.y, current.position.z),
                Quaternion.Euler(0f, 0f, pose.Angle));
        }
    }
}
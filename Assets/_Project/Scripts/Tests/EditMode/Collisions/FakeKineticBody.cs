using Asteroids.Core;
using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Collisions.Tests
{
    internal sealed class FakeKineticBody : IKineticBody
    {
        public FakeKineticBody(Vector2 velocity, float minKnockbackSpeed = 0f)
        {
            Velocity = velocity;
            MinKnockbackSpeed = minKnockbackSpeed;
        }

        public Vector2 Velocity { get; set; }
        public float MinKnockbackSpeed { get; set; }
        public int ChangeCount { get; private set; }

        public void AddVelocity(Vector2 delta)
        {
            Velocity += delta;
            ChangeCount++;
        }
    }

    internal sealed class TestEntity : IEntity
    {
        public TestEntity(Vector2 position) => Pose = new TestPose(position);

        public IPose Pose { get; }
    }

    /// <summary>Collider owned by an entity, for tests of what reacts to collisions.</summary>
    internal sealed class EntityCollider : ICollider
    {
        public EntityCollider(object owner) => Owner = owner;

        public object Owner { get; }
        public CollisionLayer Layer => CollisionLayer.All;
        public CollisionLayer Mask => CollisionLayer.All;
        public IShape Shape { get; } = new UnknownShape();
    }
}

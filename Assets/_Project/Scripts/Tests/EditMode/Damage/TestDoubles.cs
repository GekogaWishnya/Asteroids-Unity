using System.Collections.Generic;
using Asteroids.Collisions;
using Asteroids.Core;
using UnityEngine;

namespace Asteroids.Damage.Tests
{
    internal sealed class TestPose : IPose
    {
        public Vector2 Position { get; set; }
        public float Angle { get; set; }
        public Vector2 Up => Vector2.up;
        public Vector2 Right => Vector2.right;
    }

    internal sealed class TestEntity : IEntity
    {
        public IPose Pose { get; } = new TestPose();
    }

    internal sealed class TestShape : IShape { }

    internal sealed class TestCollider : ICollider
    {
        public TestCollider(object owner) => Owner = owner;

        public object Owner { get; }
        public CollisionLayer Layer => CollisionLayer.All;
        public CollisionLayer Mask => CollisionLayer.All;
        public IShape Shape { get; } = new TestShape();
    }

    internal sealed class RecordingDamageable : IDamageable
    {
        public List<int> Hits { get; } = new();

        public void TakeDamage(int amount) => Hits.Add(amount);
    }

    internal sealed class TestSource : IDamageSource
    {
        public TestSource(int damage) => Damage = damage;

        public int Damage { get; set; }
    }
}

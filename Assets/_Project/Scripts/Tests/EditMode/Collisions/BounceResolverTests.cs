using System;
using Asteroids.Collisions.Response;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Collisions.Tests
{
    [TestFixture]
    public sealed class BounceResolverTests
    {
        private const float Tolerance = 1e-4f;

        private KineticBodyRegistry _registry;
        private BounceResolver _resolver;
        private TestEntity _left;
        private TestEntity _right;
        private EntityCollider _leftCollider;
        private EntityCollider _rightCollider;

        [SetUp]
        public void SetUp()
        {
            _registry = new KineticBodyRegistry();
            _resolver = new BounceResolver(_registry);
            _left = new TestEntity(new Vector2(-0.9f, 0f));
            _right = new TestEntity(new Vector2(0.9f, 0f));
            _leftCollider = new EntityCollider(_left);
            _rightCollider = new EntityCollider(_right);
        }

        /// <summary>Contact as the left collider sees it: the normal points from right to left.</summary>
        private static Contact LeftContact(float depth = 0.2f) => new(Vector2.left, depth);

        /// <summary>Both events of a new contact, in the order the collision system sends them.</summary>
        private void Enter(Contact leftContact)
        {
            _resolver.Entered(_leftCollider, _rightCollider, leftContact);
            _resolver.Entered(_rightCollider, _leftCollider, leftContact.Inverted);
        }

        private (FakeKineticBody Left, FakeKineticBody Right) Register(FakeKineticBody left, FakeKineticBody right)
        {
            _registry.Register(_left, left);
            _registry.Register(_right, right);
            return (left, right);
        }

        private static void AssertVector(Vector2 actual, float x, float y)
        {
            Assert.That(actual.x, Is.EqualTo(x).Within(Tolerance));
            Assert.That(actual.y, Is.EqualTo(y).Within(Tolerance));
        }

        [Test]
        public void HeadOn_BothReflectBackTheWayTheyCame()
        {
            var (left, right) = Register(
                new FakeKineticBody(new Vector2(3f, 0f)), new FakeKineticBody(new Vector2(-1f, 0f)));

            Enter(LeftContact());

            AssertVector(left.Velocity, -3f, 0f);
            AssertVector(right.Velocity, 1f, 0f);
        }

        [Test]
        public void Glancing_ReflectsOffTheNormalAndKeepsTheSpeed()
        {
            // Comes in at 45 degrees, leaves at 45 degrees on the other side of the normal.
            var (left, _) = Register(new FakeKineticBody(new Vector2(2f, 2f)), new FakeKineticBody(Vector2.zero));

            Enter(LeftContact());

            AssertVector(left.Velocity, -2f, 2f);
        }

        [Test]
        public void StandingStill_IsKnockedAwayAtTheMinimumSpeed()
        {
            var (left, right) = Register(
                new FakeKineticBody(Vector2.zero, minKnockbackSpeed: 5f), new FakeKineticBody(new Vector2(-2f, 0f)));

            Enter(LeftContact());

            AssertVector(left.Velocity, -5f, 0f);
            AssertVector(right.Velocity, 2f, 0f);
        }

        [Test]
        public void ReflectionSlowerThanTheMinimum_IsRaisedToIt()
        {
            var (left, _) = Register(
                new FakeKineticBody(new Vector2(1f, 3f), minKnockbackSpeed: 4f), new FakeKineticBody(Vector2.zero));

            Enter(LeftContact());

            // Along the normal it leaves at 4 instead of the reflected 1; the tangential 3 is kept.
            AssertVector(left.Velocity, -4f, 3f);
        }

        [Test]
        public void ReflectionFasterThanTheMinimum_IsKept()
        {
            var (left, _) = Register(
                new FakeKineticBody(new Vector2(6f, 0f), minKnockbackSpeed: 4f), new FakeKineticBody(Vector2.zero));

            Enter(LeftContact());

            AssertVector(left.Velocity, -6f, 0f);
        }

        [Test]
        public void BodyMovingAwayAlready_IsNotTurnedAround()
        {
            // The left one catches up with the right one from behind.
            var (left, right) = Register(
                new FakeKineticBody(new Vector2(3f, 0f)), new FakeKineticBody(new Vector2(1f, 0f)));

            Enter(LeftContact());

            AssertVector(left.Velocity, -3f, 0f);
            AssertVector(right.Velocity, 1f, 0f);
        }

        [Test]
        public void BothEventsOfAContact_ResolveItOnce()
        {
            var (left, right) = Register(
                new FakeKineticBody(new Vector2(2f, 0f)), new FakeKineticBody(new Vector2(-2f, 0f)));

            Enter(LeftContact());

            Assert.That(left.ChangeCount, Is.EqualTo(1));
            Assert.That(right.ChangeCount, Is.EqualTo(1));
        }

        [Test]
        public void InvertedEventFirst_GivesTheSameResult()
        {
            var (left, right) = Register(
                new FakeKineticBody(new Vector2(3f, 0f)), new FakeKineticBody(new Vector2(-1f, 0f)));

            _resolver.Entered(_rightCollider, _leftCollider, LeftContact().Inverted);
            _resolver.Entered(_leftCollider, _rightCollider, LeftContact());

            AssertVector(left.Velocity, -3f, 0f);
            AssertVector(right.Velocity, 1f, 0f);
        }

        [Test]
        public void Bounce_PushesTheBodiesApartByHalfTheDepthEach()
        {
            Register(new FakeKineticBody(new Vector2(1f, 0f)), new FakeKineticBody(Vector2.zero));

            Enter(LeftContact(depth: 0.4f));

            AssertVector(_left.Pose.Position, -0.9f - 0.2f, 0f);
            AssertVector(_right.Pose.Position, 0.9f + 0.2f, 0f);
        }

        [Test]
        public void Separating_DoesNothing()
        {
            var (left, right) = Register(
                new FakeKineticBody(new Vector2(-1f, 0f), minKnockbackSpeed: 5f),
                new FakeKineticBody(new Vector2(1f, 0f)));

            Enter(LeftContact());

            Assert.That(left.ChangeCount + right.ChangeCount, Is.EqualTo(0));
            AssertVector(_left.Pose.Position, -0.9f, 0f);
        }

        [Test]
        public void Stayed_WhileClosingIn_Bounces()
        {
            var (left, _) = Register(new FakeKineticBody(new Vector2(2f, 0f)), new FakeKineticBody(Vector2.zero));

            _resolver.Stayed(_leftCollider, _rightCollider, LeftContact());

            AssertVector(left.Velocity, -2f, 0f);
        }

        [Test]
        public void OneSideNotABody_DoesNothing()
        {
            var left = new FakeKineticBody(new Vector2(2f, 0f));
            _registry.Register(_left, left);

            Enter(LeftContact());

            Assert.That(left.ChangeCount, Is.EqualTo(0));
            AssertVector(_left.Pose.Position, -0.9f, 0f);
        }

        [Test]
        public void OwnerNotAnEntity_DoesNothing()
        {
            var left = new FakeKineticBody(new Vector2(2f, 0f));
            _registry.Register(_left, left);

            _resolver.Entered(_leftCollider, new EntityCollider("wall"), LeftContact());

            Assert.That(left.ChangeCount, Is.EqualTo(0));
        }

        [Test]
        public void Construct_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new BounceResolver(null));
        }
    }
}

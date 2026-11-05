using System;
using Asteroids.Core.IConfigs;
using Asteroids.Physics;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Player.Tests
{
    /// <summary>
    /// <see cref="ShipTarget"/>: the tracked ship as enemies see it. No ship and a protected ship both read as
    /// no target, with zero position and velocity.
    /// </summary>
    [TestFixture]
    public sealed class ShipTargetTests
    {
        private sealed class TestConfig : IMotionConfig
        {
            public float MaxSpeed => 100f;
            public float LinearDamping => 0f;
        }

        private static InertialMotion MovingAt(Vector2 velocity)
        {
            var motion = new InertialMotion(new TestConfig());
            motion.AddVelocity(velocity);
            return motion;
        }

        [Test]
        public void Constructor_Null_Throws() =>
            Assert.Throws<ArgumentNullException>(() => new ShipTarget(null));

        [Test]
        public void NoShip_HasNoTarget()
        {
            var target = new ShipTarget(new ShipTracker());

            Assert.That(target.HasTarget, Is.False);
            Assert.That(target.Position, Is.EqualTo(Vector2.zero));
            Assert.That(target.Velocity, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void TrackedShip_FollowsThePoseAndTheMotion()
        {
            var tracker = new ShipTracker();
            var target = new ShipTarget(tracker);
            var pose = new BodyPose { Position = new Vector2(1f, 2f) };
            tracker.Track(pose, MovingAt(new Vector2(3f, 4f)), new FakeProtectionState());

            pose.Position = new Vector2(5f, 6f);

            Assert.That(target.HasTarget, Is.True);
            Assert.That(target.Position, Is.EqualTo(new Vector2(5f, 6f)));
            Assert.That(target.Velocity, Is.EqualTo(new Vector2(3f, 4f)));
        }

        [Test]
        public void ProtectedShip_HasNoTargetUntilProtectionEnds()
        {
            var tracker = new ShipTracker();
            var target = new ShipTarget(tracker);
            var protection = new FakeProtectionState { IsProtected = true };
            tracker.Track(new BodyPose { Position = new Vector2(1f, 2f) }, MovingAt(new Vector2(3f, 4f)), protection);

            Assert.That(target.HasTarget, Is.False);
            Assert.That(target.Position, Is.EqualTo(Vector2.zero));
            Assert.That(target.Velocity, Is.EqualTo(Vector2.zero));

            protection.IsProtected = false;

            Assert.That(target.HasTarget, Is.True);
            Assert.That(target.Position, Is.EqualTo(new Vector2(1f, 2f)));
        }

        [Test]
        public void TrackingEnds_HasNoTarget()
        {
            var tracker = new ShipTracker();
            var target = new ShipTarget(tracker);
            var handle = tracker.Track(
                new BodyPose { Position = Vector2.one }, MovingAt(Vector2.one), new FakeProtectionState());

            handle.Dispose();

            Assert.That(target.HasTarget, Is.False);
            Assert.That(target.Position, Is.EqualTo(Vector2.zero));
        }
    }
}

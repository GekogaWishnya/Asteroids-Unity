using System;
using Asteroids.Physics;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Player.Tests
{
    [TestFixture]
    public sealed class ShipTargetTests
    {
        private sealed class TestConfig : Asteroids.Core.IConfigs.IMotionConfig
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
        public void New_HasNoTarget()
        {
            var target = new ShipTarget();

            Assert.That(target.HasTarget, Is.False);
            Assert.That(target.Position, Is.EqualTo(Vector2.zero));
            Assert.That(target.Velocity, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void Track_FollowsThePoseAndTheMotion()
        {
            var target = new ShipTarget();
            var pose = new BodyPose { Position = new Vector2(1f, 2f) };
            var motion = MovingAt(new Vector2(3f, 4f));

            target.Track(pose, motion, new FakeProtectionState());
            pose.Position = new Vector2(5f, 6f);

            Assert.That(target.HasTarget, Is.True);
            Assert.That(target.Position, Is.EqualTo(new Vector2(5f, 6f)));
            Assert.That(target.Velocity, Is.EqualTo(new Vector2(3f, 4f)));
        }

        [Test]
        public void Track_WhileTheShipIsProtected_HasNoTargetUntilProtectionEnds()
        {
            var target = new ShipTarget();
            var protection = new FakeProtectionState { IsProtected = true };
            target.Track(
                new BodyPose { Position = new Vector2(1f, 2f) }, MovingAt(new Vector2(3f, 4f)), protection);

            Assert.That(target.HasTarget, Is.False);
            Assert.That(target.Position, Is.EqualTo(Vector2.zero));
            Assert.That(target.Velocity, Is.EqualTo(Vector2.zero));

            protection.IsProtected = false;

            Assert.That(target.HasTarget, Is.True);
            Assert.That(target.Position, Is.EqualTo(new Vector2(1f, 2f)));
        }

        [Test]
        public void Track_NullArguments_Throw()
        {
            var target = new ShipTarget();

            var protection = new FakeProtectionState();

            Assert.Throws<ArgumentNullException>(() => target.Track(null, MovingAt(Vector2.zero), protection));
            Assert.Throws<ArgumentNullException>(() => target.Track(new BodyPose(), null, protection));
            Assert.Throws<ArgumentNullException>(() => target.Track(new BodyPose(), MovingAt(Vector2.zero), null));
        }

        [Test]
        public void Track_WhileTracking_Throws()
        {
            var target = new ShipTarget();
            target.Track(new BodyPose(), MovingAt(Vector2.zero), new FakeProtectionState());

            Assert.Throws<InvalidOperationException>(
                () => target.Track(new BodyPose(), MovingAt(Vector2.zero), new FakeProtectionState()));
        }

        [Test]
        public void Dispose_ClearsTheTargetAndAllowsANewOne()
        {
            var target = new ShipTarget();
            var handle = target.Track(
                new BodyPose { Position = Vector2.one }, MovingAt(Vector2.one), new FakeProtectionState());

            handle.Dispose();

            Assert.That(target.HasTarget, Is.False);
            Assert.That(target.Position, Is.EqualTo(Vector2.zero));
            Assert.DoesNotThrow(() => target.Track(new BodyPose(), MovingAt(Vector2.zero), new FakeProtectionState()));
        }

        [Test]
        public void Dispose_StaleHandle_DoesNotClearTheNewerTarget()
        {
            var target = new ShipTarget();
            var stale = target.Track(new BodyPose(), MovingAt(Vector2.zero), new FakeProtectionState());
            stale.Dispose();
            target.Track(new BodyPose(), MovingAt(Vector2.zero), new FakeProtectionState());

            stale.Dispose();

            Assert.That(target.HasTarget, Is.True);
        }
    }
}

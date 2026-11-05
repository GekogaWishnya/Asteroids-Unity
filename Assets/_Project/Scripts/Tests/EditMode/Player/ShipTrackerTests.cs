using System;
using Asteroids.Core.IConfigs;
using Asteroids.Physics;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Player.Tests
{
    /// <summary>
    /// <see cref="ShipTracker"/>: one live ship at a time, read through <see cref="ShipTracker.TryGetShip"/>,
    /// and a handle that only ever clears the ship it started tracking.
    /// </summary>
    [TestFixture]
    public sealed class ShipTrackerTests
    {
        private sealed class TestConfig : IMotionConfig
        {
            public float MaxSpeed => 100f;
            public float LinearDamping => 0f;
        }

        private static InertialMotion Motion() => new(new TestConfig());

        [Test]
        public void New_HasNoShip()
        {
            Assert.That(new ShipTracker().TryGetShip(out var ship), Is.False);
            Assert.That(ship.Pose, Is.Null);
        }

        [Test]
        public void Track_ReadsTheSameObjectsItWasGiven()
        {
            var tracker = new ShipTracker();
            var pose = new BodyPose();
            var motion = Motion();
            var protection = new FakeProtectionState();

            tracker.Track(pose, motion, protection);

            Assert.That(tracker.TryGetShip(out var ship), Is.True);
            Assert.That(ship.Pose, Is.SameAs(pose));
            Assert.That(ship.Motion, Is.SameAs(motion));
            Assert.That(ship.Protection, Is.SameAs(protection));
        }

        [Test]
        public void Track_NullArguments_Throw()
        {
            var tracker = new ShipTracker();
            var protection = new FakeProtectionState();

            Assert.Throws<ArgumentNullException>(() => tracker.Track(null, Motion(), protection));
            Assert.Throws<ArgumentNullException>(() => tracker.Track(new BodyPose(), null, protection));
            Assert.Throws<ArgumentNullException>(() => tracker.Track(new BodyPose(), Motion(), null));
        }

        [Test]
        public void Track_WhileTracking_Throws()
        {
            var tracker = new ShipTracker();
            tracker.Track(new BodyPose(), Motion(), new FakeProtectionState());

            Assert.Throws<InvalidOperationException>(
                () => tracker.Track(new BodyPose(), Motion(), new FakeProtectionState()));
        }

        [Test]
        public void Dispose_ClearsTheShipAndAllowsANewOne()
        {
            var tracker = new ShipTracker();
            var handle = tracker.Track(new BodyPose { Position = Vector2.one }, Motion(), new FakeProtectionState());

            handle.Dispose();

            Assert.That(tracker.TryGetShip(out _), Is.False);
            Assert.DoesNotThrow(() => tracker.Track(new BodyPose(), Motion(), new FakeProtectionState()));
        }

        [Test]
        public void Dispose_StaleHandle_DoesNotClearTheNewerShip()
        {
            var tracker = new ShipTracker();
            var stale = tracker.Track(new BodyPose(), Motion(), new FakeProtectionState());
            stale.Dispose();
            tracker.Track(new BodyPose(), Motion(), new FakeProtectionState());

            stale.Dispose();

            Assert.That(tracker.TryGetShip(out _), Is.True);
        }
    }
}

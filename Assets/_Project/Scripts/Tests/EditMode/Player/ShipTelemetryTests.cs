using System;
using Asteroids.Core.IConfigs;
using Asteroids.Physics;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Player.Tests
{
    /// <summary>
    /// <see cref="ShipTelemetry"/>: the tracked ship as the HUD sees it. It reads the ship whether or not it is
    /// protected, which is exactly where it differs from <see cref="ShipTarget"/>, and reads zero without one.
    /// </summary>
    [TestFixture]
    public sealed class ShipTelemetryTests
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

        private static void AssertReadsZero(ShipTelemetry telemetry)
        {
            Assert.That(telemetry.HasShip, Is.False);
            Assert.That(telemetry.Position, Is.EqualTo(Vector2.zero));
            Assert.That(telemetry.Angle, Is.EqualTo(0f));
            Assert.That(telemetry.Speed, Is.EqualTo(0f));
        }

        [Test]
        public void Constructor_Null_Throws() =>
            Assert.Throws<ArgumentNullException>(() => new ShipTelemetry(null));

        [Test]
        public void NoShip_HasNoShipAndReadsZero() => AssertReadsZero(new ShipTelemetry(new ShipTracker()));

        [Test]
        public void TrackedShip_ReadsThePoseAndTheSpeed()
        {
            var tracker = new ShipTracker();
            var telemetry = new ShipTelemetry(tracker);
            var pose = new BodyPose { Position = new Vector2(1f, 2f), Angle = 30f };
            tracker.Track(pose, MovingAt(new Vector2(3f, 4f)), new FakeProtectionState());

            pose.Position = new Vector2(-5f, 6f);
            pose.Angle = 90f;

            Assert.That(telemetry.HasShip, Is.True);
            Assert.That(telemetry.Position, Is.EqualTo(new Vector2(-5f, 6f)));
            Assert.That(telemetry.Angle, Is.EqualTo(90f));
            Assert.That(telemetry.Speed, Is.EqualTo(5f).Within(1e-5f));
        }

        [Test]
        public void ProtectedShip_KeepsReading()
        {
            var tracker = new ShipTracker();
            var telemetry = new ShipTelemetry(tracker);
            tracker.Track(
                new BodyPose { Position = new Vector2(1f, 2f), Angle = 45f },
                MovingAt(new Vector2(0f, 2f)),
                new FakeProtectionState { IsProtected = true });

            Assert.That(telemetry.HasShip, Is.True);
            Assert.That(telemetry.Position, Is.EqualTo(new Vector2(1f, 2f)));
            Assert.That(telemetry.Angle, Is.EqualTo(45f));
            Assert.That(telemetry.Speed, Is.EqualTo(2f).Within(1e-5f));
        }

        [Test]
        public void TrackingEnds_HasNoShipAndReadsZero()
        {
            var tracker = new ShipTracker();
            var telemetry = new ShipTelemetry(tracker);
            var handle = tracker.Track(
                new BodyPose { Position = Vector2.one, Angle = 10f }, MovingAt(Vector2.one), new FakeProtectionState());

            handle.Dispose();

            AssertReadsZero(telemetry);
        }
    }
}

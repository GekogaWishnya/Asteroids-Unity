using System;
using System.Collections.Generic;
using Asteroids.Physics;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Enemies.Tests
{
    [TestFixture]
    public sealed class UfoSteeringTests
    {
        private const float Tolerance = 1e-3f;

        private sealed class RecordingDriver : IMotionDriver
        {
            public List<Vector2> Accelerations { get; } = new();
            public void Accelerate(Vector2 acceleration) => Accelerations.Add(acceleration);
            public void AddVelocity(Vector2 delta) { }
        }

        private sealed class Rig
        {
            public readonly TestUfoType Config = new() { Thrust = 6f, TurnSpeed = 90f };
            public readonly BodyPose Pose = new();
            public readonly RecordingDriver Driver = new();
            public readonly FakePlayerTarget Target = FakePlayerTarget.At(0f, 10f);
            public readonly UfoSteering Steering;

            public Rig() => Steering = new UfoSteering(Config, Pose, Driver, Target);
        }

        private static void AssertVector(Vector2 actual, float x, float y)
        {
            Assert.That(actual.x, Is.EqualTo(x).Within(Tolerance));
            Assert.That(actual.y, Is.EqualTo(y).Within(Tolerance));
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            var rig = new Rig();

            Assert.Throws<ArgumentNullException>(() => new UfoSteering(null, rig.Pose, rig.Driver, rig.Target));
            Assert.Throws<ArgumentNullException>(() => new UfoSteering(rig.Config, null, rig.Driver, rig.Target));
            Assert.Throws<ArgumentNullException>(() => new UfoSteering(rig.Config, rig.Pose, null, rig.Target));
            Assert.Throws<ArgumentNullException>(() => new UfoSteering(rig.Config, rig.Pose, rig.Driver, null));
        }

        [TestCase(0f, 1f, 0f)]
        [TestCase(-1f, 0f, 90f)]
        [TestCase(0f, -1f, 180f)]
        [TestCase(1f, 0f, -90f)]
        public void AngleOf_MatchesThePoseConvention(float x, float y, float expected)
        {
            var angle = UfoSteering.AngleOf(new Vector2(x, y));

            Assert.That(Mathf.DeltaAngle(angle, expected), Is.EqualTo(0f).Within(Tolerance));

            var pose = new BodyPose { Angle = angle };
            AssertVector(pose.Up, x, y);
        }

        [Test]
        public void Tick_TargetStraightAhead_ThrustsAlongTheNoseWithoutTurning()
        {
            var rig = new Rig();

            rig.Steering.Tick(0.1f);

            Assert.That(rig.Pose.Angle, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(rig.Driver.Accelerations, Has.Count.EqualTo(1));
            AssertVector(rig.Driver.Accelerations[0], 0f, 6f);
        }

        [Test]
        public void Tick_TargetBehind_TurnsByAtMostTheTurnSpeed()
        {
            var rig = new Rig();
            rig.Target.Position = new Vector2(0f, -10f);

            rig.Steering.Tick(0.5f);

            Assert.That(Mathf.Abs(Mathf.DeltaAngle(0f, rig.Pose.Angle)), Is.EqualTo(45f).Within(Tolerance));
        }

        [Test]
        public void Tick_TargetWithinOneTicksTurn_FacesItExactlyAndThrustsAtIt()
        {
            var rig = new Rig();
            rig.Target.Position = new Vector2(-10f, 0f);

            rig.Steering.Tick(1f);

            Assert.That(rig.Pose.Angle, Is.EqualTo(90f).Within(Tolerance));
            AssertVector(rig.Driver.Accelerations[0], -6f, 0f);
        }

        [Test]
        public void Tick_AimsAtTheRawPosition_EvenWhenTheWrapWouldBeShorter()
        {
            // The ship is far to the left. Across a wrap it could be close on the right; the UFO ignores that.
            var rig = new Rig();
            rig.Pose.Position = new Vector2(7f, 0f);
            rig.Target.Position = new Vector2(-7f, 0f);

            rig.Steering.Tick(1f);

            Assert.That(rig.Pose.Angle, Is.EqualTo(90f).Within(Tolerance));
        }

        [Test]
        public void Tick_TargetOnTopOfTheUfo_KeepsTheHeading()
        {
            var rig = new Rig();
            rig.Pose.Angle = 30f;
            rig.Target.Position = Vector2.zero;

            rig.Steering.Tick(1f);

            Assert.That(rig.Pose.Angle, Is.EqualTo(30f).Within(Tolerance));
        }

        [Test]
        public void Tick_NoTarget_NeitherTurnsNorThrusts()
        {
            var rig = new Rig();
            rig.Target.HasTarget = false;
            rig.Target.Position = new Vector2(-10f, 0f);

            rig.Steering.Tick(1f);

            Assert.That(rig.Pose.Angle, Is.EqualTo(0f));
            Assert.That(rig.Driver.Accelerations, Is.Empty);
        }

        [Test]
        public void Tick_WhileStunned_NeitherTurnsNorThrustsAndCountsDown()
        {
            var rig = new Rig();
            rig.Target.Position = new Vector2(-10f, 0f);
            rig.Steering.Stun(1f);

            rig.Steering.Tick(0.4f);

            Assert.That(rig.Steering.IsStunned, Is.True);
            Assert.That(rig.Steering.StunRemaining, Is.EqualTo(0.6f).Within(Tolerance));
            Assert.That(rig.Pose.Angle, Is.EqualTo(0f));
            Assert.That(rig.Driver.Accelerations, Is.Empty);
        }

        [Test]
        public void Tick_AfterTheStunRunsOut_SteersAgain()
        {
            var rig = new Rig();
            rig.Steering.Stun(0.5f);

            rig.Steering.Tick(0.5f);
            rig.Steering.Tick(0.1f);

            Assert.That(rig.Steering.IsStunned, Is.False);
            Assert.That(rig.Driver.Accelerations, Has.Count.EqualTo(1));
        }

        [Test]
        public void Stun_Shorter_KeepsTheLongerStun()
        {
            var rig = new Rig();

            rig.Steering.Stun(2f);
            rig.Steering.Stun(0.5f);

            Assert.That(rig.Steering.StunRemaining, Is.EqualTo(2f));
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Stun_InvalidDuration_Throws(float seconds)
        {
            var rig = new Rig();

            Assert.Throws<ArgumentOutOfRangeException>(() => rig.Steering.Stun(seconds));
        }

        [Test]
        public void Reset_EndsTheStun()
        {
            var rig = new Rig();
            rig.Steering.Stun(2f);

            rig.Steering.Reset();

            Assert.That(rig.Steering.IsStunned, Is.False);
        }
    }
}

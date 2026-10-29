using System;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Physics.Tests
{
    [TestFixture]
    public sealed class SpinStepTests
    {
        private const float Tolerance = 1e-3f;

        [Test]
        public void Constructor_NullPose_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new SpinStep(null, new AngularMotion()));
        }

        [Test]
        public void Constructor_NullMotion_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new SpinStep(new BodyPose(), null));
        }

        [Test]
        public void Tick_PositiveVelocity_RotatesByVelocityTimesDelta()
        {
            var pose = new BodyPose { Angle = 10f };
            var step = new SpinStep(pose, new AngularMotion { Velocity = 90f });

            step.Tick(0.5f);

            Assert.That(pose.Angle, Is.EqualTo(55f).Within(Tolerance));
        }

        [Test]
        public void Tick_Repeated_Accumulates()
        {
            var pose = new BodyPose();
            var step = new SpinStep(pose, new AngularMotion { Velocity = 20f });

            step.Tick(1f);
            step.Tick(1f);
            step.Tick(1f);

            Assert.That(pose.Angle, Is.EqualTo(60f).Within(Tolerance));
        }

        [Test]
        public void Tick_NegativeVelocity_WrapsBelowZero()
        {
            var pose = new BodyPose { Angle = 10f };
            var step = new SpinStep(pose, new AngularMotion { Velocity = -90f });

            step.Tick(0.5f);

            Assert.That(pose.Angle, Is.EqualTo(325f).Within(Tolerance));
        }

        [Test]
        public void Tick_DoesNotChangePosition()
        {
            var pose = new BodyPose { Position = new Vector2(3f, -2f) };
            var step = new SpinStep(pose, new AngularMotion { Velocity = 45f });

            step.Tick(1f);

            Assert.That(pose.Position, Is.EqualTo(new Vector2(3f, -2f)));
        }

        [Test]
        public void Tick_ZeroVelocity_KeepsAngle()
        {
            var pose = new BodyPose { Angle = 33f };
            var step = new SpinStep(pose, new AngularMotion());

            step.Tick(1f);

            Assert.That(pose.Angle, Is.EqualTo(33f).Within(Tolerance));
        }

        [Test]
        public void Tick_VelocityChangedBetweenTicks_UsesCurrentVelocity()
        {
            var pose = new BodyPose();
            var motion = new AngularMotion { Velocity = 10f };
            var step = new SpinStep(pose, motion);

            step.Tick(1f);
            motion.Velocity = 40f;
            step.Tick(1f);

            Assert.That(pose.Angle, Is.EqualTo(50f).Within(Tolerance));
        }
    }
}

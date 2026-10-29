using System;
using NUnit.Framework;

namespace Asteroids.Physics.Tests
{
    [TestFixture]
    public sealed class AngularMotionTests
    {
        [Test]
        public void Velocity_ByDefault_IsZero()
        {
            Assert.That(new AngularMotion().Velocity, Is.EqualTo(0f));
        }

        [Test]
        public void Step_ReturnsVelocityTimesDeltaTime()
        {
            var motion = new AngularMotion { Velocity = 90f };

            Assert.That(motion.Step(0.5f), Is.EqualTo(45f).Within(1e-4f));
        }

        [Test]
        public void Step_NegativeVelocity_ReturnsNegativeRotation()
        {
            var motion = new AngularMotion { Velocity = -30f };

            Assert.That(motion.Step(2f), Is.EqualTo(-60f).Within(1e-4f));
        }

        [Test]
        public void Step_ZeroVelocity_ReturnsZero()
        {
            Assert.That(new AngularMotion().Step(1f), Is.EqualTo(0f));
        }

        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void Velocity_NonFinite_Throws(float value)
        {
            var motion = new AngularMotion();

            Assert.Throws<ArgumentOutOfRangeException>(() => motion.Velocity = value);
        }
    }
}

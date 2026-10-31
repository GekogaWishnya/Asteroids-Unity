using System;
using Asteroids.Core.IConfigs;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Physics.Tests
{
    [TestFixture]
    public sealed class KineticBodyTests
    {
        private sealed class TestBodyConfig : IKnockbackConfig, IMotionConfig
        {
            public float MinKnockbackSpeed { get; set; } = 5f;
            public float MaxSpeed { get; set; } = 100f;
            public float LinearDamping => 0f;
        }

        private TestBodyConfig _config;
        private InertialMotion _motion;
        private KineticBody _body;

        [SetUp]
        public void SetUp()
        {
            _config = new TestBodyConfig();
            _motion = new InertialMotion(_config);
            _body = new KineticBody(_motion, _config);
        }

        [Test]
        public void Velocity_IsTheVelocityOfTheMotion()
        {
            _motion.AddVelocity(new Vector2(3f, -1f));

            Assert.That(_body.Velocity, Is.EqualTo(new Vector2(3f, -1f)));
        }

        [Test]
        public void AddVelocity_ChangesTheVelocityOfTheMotion()
        {
            _body.AddVelocity(new Vector2(4f, 2f));

            Assert.That(_motion.Velocity, Is.EqualTo(new Vector2(4f, 2f)));
        }

        [Test]
        public void MinKnockbackSpeed_IsReadFromTheConfigEveryTime()
        {
            _config.MinKnockbackSpeed = 7f;

            Assert.That(_body.MinKnockbackSpeed, Is.EqualTo(7f));
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void MinKnockbackSpeed_NegativeOrNotFinite_IsZero(float speed)
        {
            _config.MinKnockbackSpeed = speed;

            Assert.That(_body.MinKnockbackSpeed, Is.EqualTo(0f));
        }

        [Test]
        public void Construct_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new KineticBody(null, _motion, _config));
            Assert.Throws<ArgumentNullException>(() => new KineticBody(_motion, null, _config));
            Assert.Throws<ArgumentNullException>(() => new KineticBody(_motion, _motion, null));
            Assert.Throws<ArgumentNullException>(() => new KineticBody(null, _config));
            Assert.Throws<ArgumentNullException>(() => new KineticBody(_motion, null));
        }
    }
}

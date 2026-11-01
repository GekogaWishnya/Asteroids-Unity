using System;
using Asteroids.Core.IConfigs;
using Asteroids.Physics;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Player.Tests
{
    [TestFixture]
    public sealed class ShipTargetBeaconTests
    {
        private sealed class TestConfig : IMotionConfig
        {
            public float MaxSpeed => 100f;
            public float LinearDamping => 0f;
        }

        private ShipTarget _target;
        private InertialMotion _motion;
        private Entity _ship;
        private FakeProtectionState _protection;
        private ShipTargetBeacon _beacon;

        [SetUp]
        public void SetUp()
        {
            _target = new ShipTarget();
            _motion = new InertialMotion(new TestConfig());
            _ship = new Entity(new BodyPose { Position = new Vector2(2f, 3f) });
            _protection = new FakeProtectionState();
            _beacon = new ShipTargetBeacon(_target, _motion, _protection);
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new ShipTargetBeacon(null, _motion, _protection));
            Assert.Throws<ArgumentNullException>(() => new ShipTargetBeacon(_target, null, _protection));
            Assert.Throws<ArgumentNullException>(() => new ShipTargetBeacon(_target, _motion, null));
        }

        [Test]
        public void Attach_MakesTheShipTheTarget()
        {
            _motion.AddVelocity(new Vector2(1f, 0f));

            _beacon.Attach(_ship);

            Assert.That(_target.HasTarget, Is.True);
            Assert.That(_target.Position, Is.EqualTo(new Vector2(2f, 3f)));
            Assert.That(_target.Velocity, Is.EqualTo(new Vector2(1f, 0f)));
        }

        [Test]
        public void Attach_WhileProtected_IsNoTarget()
        {
            _protection.IsProtected = true;

            _beacon.Attach(_ship);

            Assert.That(_target.HasTarget, Is.False);
        }

        [Test]
        public void Attach_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _beacon.Attach(null));
        }

        [Test]
        public void Attach_Twice_Throws()
        {
            _beacon.Attach(_ship);

            Assert.Throws<InvalidOperationException>(() => _beacon.Attach(_ship));
        }

        [Test]
        public void Detach_ClearsTheTarget()
        {
            var handle = _beacon.Attach(_ship);

            handle.Dispose();

            Assert.That(_target.HasTarget, Is.False);
        }
    }
}

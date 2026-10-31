using System;
using Asteroids.Core;
using Asteroids.Physics;
using NUnit.Framework;

namespace Asteroids.Player.Tests
{
    [TestFixture]
    public sealed class ShipControlLossTests
    {
        private const float Tolerance = 1e-4f;

        private FakeSimulationLoop _loop;
        private ShipControlLoss _loss;
        private Entity _ship;

        [SetUp]
        public void SetUp()
        {
            _loop = new FakeSimulationLoop();
            _loss = new ShipControlLoss(_loop);
            _ship = new Entity(new BodyPose());
        }

        [Test]
        public void ByDefault_IsNotLocked()
        {
            Assert.That(_loss.IsLocked, Is.False);
        }

        [Test]
        public void Attach_RegistersTheCountdownInLogic()
        {
            _loss.Attach(_ship);

            Assert.That(_loop.LiveCount(SimulationPhase.Logic), Is.EqualTo(1));
            Assert.That(_loop.Entries.Count, Is.EqualTo(1));
        }

        [Test]
        public void Lose_WhileAttached_LocksForTheDuration()
        {
            _loss.Attach(_ship);

            _loss.Lose(3f);
            _loop.Tick(2.5f);
            Assert.That(_loss.IsLocked, Is.True);

            _loop.Tick(0.5f);
            Assert.That(_loss.IsLocked, Is.False);
        }

        [Test]
        public void Lose_ShorterThanTheRunningLock_KeepsTheLongerOne()
        {
            _loss.Attach(_ship);
            _loss.Lose(3f);

            _loss.Lose(1f);

            Assert.That(_loss.Remaining, Is.EqualTo(3f).Within(Tolerance));
        }

        [Test]
        public void Lose_LongerThanTheRunningLock_ExtendsIt()
        {
            _loss.Attach(_ship);
            _loss.Lose(1f);

            _loss.Lose(3f);

            Assert.That(_loss.Remaining, Is.EqualTo(3f).Within(Tolerance));
        }

        [Test]
        public void Lose_Zero_DoesNotLock()
        {
            _loss.Attach(_ship);

            _loss.Lose(0f);

            Assert.That(_loss.IsLocked, Is.False);
        }

        [Test]
        public void Lose_WhileDetached_DoesNothing()
        {
            _loss.Lose(3f);

            Assert.That(_loss.IsLocked, Is.False);
        }

        [Test]
        public void Detach_GivesControlBackAtOnce()
        {
            var attachment = _loss.Attach(_ship);
            _loss.Lose(3f);

            attachment.Dispose();

            Assert.That(_loss.IsLocked, Is.False);
            Assert.That(_loop.LiveCount(SimulationPhase.Logic), Is.EqualTo(0));
        }

        [Test]
        public void Lose_AfterDetach_DoesNothing()
        {
            _loss.Attach(_ship).Dispose();

            _loss.Lose(3f);

            Assert.That(_loss.IsLocked, Is.False);
        }

        [Test]
        public void Attach_Twice_Throws()
        {
            _loss.Attach(_ship);

            Assert.Throws<InvalidOperationException>(() => _loss.Attach(_ship));
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Lose_InvalidDuration_Throws(float seconds)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _loss.Lose(seconds));
        }

        [Test]
        public void NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new ShipControlLoss(null));
            Assert.Throws<ArgumentNullException>(() => _loss.Attach(null));
        }
    }
}

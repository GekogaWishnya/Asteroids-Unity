using System;
using NUnit.Framework;

namespace Asteroids.Player.Tests
{
    [TestFixture]
    public sealed class GatedInputTests
    {
        private TestShipInput _input;
        private TestControlLock _lock;

        [SetUp]
        public void SetUp()
        {
            _input = new TestShipInput { Turn = -1f, Thrust = 0.5f, IsFiring = true };
            _lock = new TestControlLock();
        }

        [Test]
        public void Movement_Unlocked_PassesTheInputThrough()
        {
            var gated = new GatedMovementInput(_input, _lock);

            Assert.That(gated.Turn, Is.EqualTo(-1f));
            Assert.That(gated.Thrust, Is.EqualTo(0.5f));
        }

        [Test]
        public void Movement_Locked_ReadsAsIdle()
        {
            var gated = new GatedMovementInput(_input, _lock);

            _lock.IsLocked = true;

            Assert.That(gated.Turn, Is.EqualTo(0f));
            Assert.That(gated.Thrust, Is.EqualTo(0f));
        }

        [Test]
        public void Fire_Unlocked_PassesTheInputThrough()
        {
            Assert.That(new GatedFireInput(_input, _lock).IsFiring, Is.True);
        }

        [Test]
        public void Fire_Locked_ReadsAsReleased()
        {
            var gated = new GatedFireInput(_input, _lock);

            _lock.IsLocked = true;

            Assert.That(gated.IsFiring, Is.False);
        }

        [Test]
        public void Construct_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new GatedMovementInput(null, _lock));
            Assert.Throws<ArgumentNullException>(() => new GatedMovementInput(_input, null));
            Assert.Throws<ArgumentNullException>(() => new GatedFireInput(null, _lock));
            Assert.Throws<ArgumentNullException>(() => new GatedFireInput(_input, null));
        }
    }
}

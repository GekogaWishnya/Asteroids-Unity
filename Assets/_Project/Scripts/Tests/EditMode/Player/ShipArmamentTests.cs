using System;
using Asteroids.Physics;
using NUnit.Framework;

namespace Asteroids.Player.Tests
{
    [TestFixture]
    public sealed class ShipArmamentTests
    {
        private FakeWeapon _weapon;
        private TestShipInput _trigger;
        private TestControlLock _lock;
        private Entity _ship;

        [SetUp]
        public void SetUp()
        {
            _weapon = new FakeWeapon();
            _trigger = new TestShipInput();
            _lock = new TestControlLock();
            _ship = new Entity(new BodyPose());
        }

        private ShipArmament Create() => new(_weapon, _trigger, _lock);

        [Test]
        public void Construct_DoesNotAttachTheWeapon()
        {
            Create();

            Assert.That(_weapon.Attachments, Is.Empty);
        }

        [Test]
        public void Attach_MountsTheWeaponOnTheShipWithATriggerThatFollowsTheInput()
        {
            Create().Attach(_ship);

            Assert.That(_weapon.Attachments.Count, Is.EqualTo(1));
            Assert.That(_weapon.Attachments[0].Carrier, Is.SameAs(_ship));

            var trigger = _weapon.Attachments[0].Trigger;
            _trigger.IsFiring = true;
            Assert.That(trigger.IsFiring, Is.True);
            _trigger.IsFiring = false;
            Assert.That(trigger.IsFiring, Is.False);
        }

        [Test]
        public void Trigger_WhileLocked_DoesNotFire()
        {
            Create().Attach(_ship);
            _trigger.IsFiring = true;

            _lock.IsLocked = true;

            Assert.That(_weapon.Attachments[0].Trigger.IsFiring, Is.False);
        }

        [Test]
        public void Dispose_TakesTheWeaponOff()
        {
            Create().Attach(_ship).Dispose();

            Assert.That(_weapon.LiveCount, Is.EqualTo(0));
        }

        [Test]
        public void Attach_Twice_Throws()
        {
            var armament = Create();
            armament.Attach(_ship);

            Assert.Throws<InvalidOperationException>(() => armament.Attach(_ship));
        }

        [Test]
        public void Attach_NullShip_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Create().Attach(null));
        }

        [Test]
        public void Construct_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new ShipArmament(null, _trigger, _lock));
            Assert.Throws<ArgumentNullException>(() => new ShipArmament(_weapon, null, _lock));
            Assert.Throws<ArgumentNullException>(() => new ShipArmament(_weapon, _trigger, null));
        }
    }
}

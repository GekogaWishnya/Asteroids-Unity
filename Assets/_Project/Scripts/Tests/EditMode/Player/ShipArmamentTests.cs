using System;
using System.Collections.Generic;
using Asteroids.Core;
using Asteroids.Physics;
using NUnit.Framework;

namespace Asteroids.Player.Tests
{
    [TestFixture]
    public sealed class ShipArmamentTests
    {
        private FakeWeapon _primary;
        private FakeWeapon _alternate;
        private FakeLoadout _loadout;
        private TestShipInput _input;
        private TestControlLock _lock;
        private Entity _ship;

        [SetUp]
        public void SetUp()
        {
            _primary = new FakeWeapon();
            _alternate = new FakeWeapon();
            _loadout = new FakeLoadout { Primary = _primary, Alternate = _alternate };
            _input = new TestShipInput();
            _lock = new TestControlLock();
            _ship = new Entity(new BodyPose());
        }

        private ShipArmament Create() => new(_loadout, _input, _lock);

        [Test]
        public void Construct_DoesNotAttachTheWeapons()
        {
            Create();

            Assert.That(_primary.Attachments, Is.Empty);
            Assert.That(_alternate.Attachments, Is.Empty);
        }

        [Test]
        public void Attach_MountsEveryWeaponOnTheShip()
        {
            Create().Attach(_ship);

            Assert.That(_primary.LiveCount, Is.EqualTo(1));
            Assert.That(_alternate.LiveCount, Is.EqualTo(1));
            Assert.That(_primary.Attachments[0].Carrier, Is.SameAs(_ship));
            Assert.That(_alternate.Attachments[0].Carrier, Is.SameAs(_ship));
        }

        [Test]
        public void Attach_MountsThePrimaryWeaponFirst()
        {
            var order = new List<string>();
            _primary.OnAttach = () => order.Add("Primary");
            _alternate.OnAttach = () => order.Add("Alternate");

            Create().Attach(_ship);

            Assert.That(order, Is.EqualTo(new[] { "Primary", "Alternate" }));
        }

        [Test]
        public void Attach_EachWeaponFollowsTheTriggerOfItsSlot()
        {
            Create().Attach(_ship);
            var primary = _primary.Attachments[0].Trigger;
            var alternate = _alternate.Attachments[0].Trigger;

            _input.IsFiring = true;
            Assert.That(primary.IsFiring, Is.True);
            Assert.That(alternate.IsFiring, Is.False);

            _input.IsFiring = false;
            _input.Alternate.IsFiring = true;
            Assert.That(primary.IsFiring, Is.False);
            Assert.That(alternate.IsFiring, Is.True);
        }

        [Test]
        public void Triggers_WhileLocked_DoNotFire()
        {
            Create().Attach(_ship);
            _input.IsFiring = true;
            _input.Alternate.IsFiring = true;

            _lock.IsLocked = true;

            Assert.That(_primary.Attachments[0].Trigger.IsFiring, Is.False);
            Assert.That(_alternate.Attachments[0].Trigger.IsFiring, Is.False);
        }

        [Test]
        public void Attach_OnlyAnAlternateWeapon_MountsIt()
        {
            _loadout.Primary = null;

            Create().Attach(_ship);

            Assert.That(_alternate.LiveCount, Is.EqualTo(1));
            Assert.That(_primary.Attachments, Is.Empty);
        }

        [Test]
        public void Attach_EmptyLoadout_MountsNothingAndStillTakesTheSlot()
        {
            _loadout.Primary = null;
            _loadout.Alternate = null;
            var armament = Create();

            var attachment = armament.Attach(_ship);

            Assert.That(attachment, Is.Not.Null);
            Assert.Throws<InvalidOperationException>(() => armament.Attach(_ship));
        }

        [Test]
        public void Attach_ASecondWeaponFails_TakesTheFirstOffAgain()
        {
            _alternate.OnAttach = () => throw new InvalidOperationException("mount failed");

            Assert.Throws<InvalidOperationException>(() => Create().Attach(_ship));

            Assert.That(_primary.LiveCount, Is.EqualTo(0));
        }

        [Test]
        public void Dispose_TakesEveryWeaponOff()
        {
            Create().Attach(_ship).Dispose();

            Assert.That(_primary.LiveCount, Is.EqualTo(0));
            Assert.That(_alternate.LiveCount, Is.EqualTo(0));
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
            Assert.Throws<ArgumentNullException>(() => new ShipArmament(null, _input, _lock));
            Assert.Throws<ArgumentNullException>(() => new ShipArmament(_loadout, null, _lock));
            Assert.Throws<ArgumentNullException>(() => new ShipArmament(_loadout, _input, null));
        }

        [Test]
        public void Reset_ResetsEveryWeapon()
        {
            Create().Reset();

            Assert.That(_primary.ResetCount, Is.EqualTo(1));
            Assert.That(_alternate.ResetCount, Is.EqualTo(1));
        }

        [Test]
        public void Reset_AWeaponFails_ResetsTheOthersAndRethrows()
        {
            var armament = Create();
            _primary.FailOnReset = true;

            Assert.Throws<InvalidOperationException>(() => armament.Reset());

            Assert.That(_alternate.ResetCount, Is.EqualTo(1));
        }
    }
}

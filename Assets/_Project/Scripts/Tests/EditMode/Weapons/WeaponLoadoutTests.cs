using System;
using Asteroids.Core;
using Asteroids.Weapons.Laser;
using NUnit.Framework;

namespace Asteroids.Weapons.Tests
{
    [TestFixture]
    public sealed class WeaponLoadoutTests
    {
        private static IWeapon NewWeapon() =>
            new LaserWeapon(new TestLaserConfig(), new FakeSimulationLoop(), new FakeBeamView(),
                new LaserBeamRegistry(), new FakeDamageLookup());

        [Test]
        public void TryGetWeapon_ReturnsTheWeaponOfEachSlot()
        {
            var primary = NewWeapon();
            var alternate = NewWeapon();
            var loadout = new WeaponLoadout(primary, alternate);

            Assert.That(loadout.TryGetWeapon(WeaponSlot.Primary, out var first), Is.True);
            Assert.That(first, Is.SameAs(primary));
            Assert.That(loadout.TryGetWeapon(WeaponSlot.Alternate, out var second), Is.True);
            Assert.That(second, Is.SameAs(alternate));
        }

        [Test]
        public void TryGetWeapon_EmptySlot_IsFalse()
        {
            var loadout = new WeaponLoadout(null, NewWeapon());

            Assert.That(loadout.TryGetWeapon(WeaponSlot.Primary, out var weapon), Is.False);
            Assert.That(weapon, Is.Null);
        }

        [Test]
        public void Empty_HasNoWeapons()
        {
            var loadout = WeaponLoadout.Empty();

            Assert.That(loadout.TryGetWeapon(WeaponSlot.Primary, out _), Is.False);
            Assert.That(loadout.TryGetWeapon(WeaponSlot.Alternate, out _), Is.False);
        }

        [Test]
        public void Construct_TheSameWeaponInBothSlots_Throws()
        {
            var weapon = NewWeapon();

            Assert.Throws<ArgumentException>(() => new WeaponLoadout(weapon, weapon));
        }

        [Test]
        public void TryGetWeapon_UnknownSlot_Throws() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => WeaponLoadout.Empty().TryGetWeapon((WeaponSlot)7, out _));
    }
}

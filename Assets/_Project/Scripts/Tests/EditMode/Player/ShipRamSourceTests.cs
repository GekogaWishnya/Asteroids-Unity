using System;
using NUnit.Framework;

namespace Asteroids.Player.Tests
{
    [TestFixture]
    public sealed class ShipRamSourceTests
    {
        [Test]
        public void Constructor_NullConfig_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ShipRamSource(null));
        }

        [Test]
        public void Damage_ReportsTheConfiguredRamDamage()
        {
            var config = new TestCombatConfig { RamDamage = 3 };

            Assert.That(new ShipRamSource(config).Damage, Is.EqualTo(3));
        }

        [Test]
        public void Damage_FollowsChangesOfTheConfig()
        {
            var config = new TestCombatConfig { RamDamage = 1 };
            var source = new ShipRamSource(config);

            config.RamDamage = 4;

            Assert.That(source.Damage, Is.EqualTo(4));
        }
    }
}

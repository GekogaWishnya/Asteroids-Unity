using System;
using Asteroids.Physics;
using NUnit.Framework;

namespace Asteroids.Player.Tests
{
    [TestFixture]
    public sealed class ShipRamTests
    {
        private TestCombatConfig _config;
        private FakeDamageRegistry _registry;
        private Entity _ship;

        [SetUp]
        public void SetUp()
        {
            _config = new TestCombatConfig { RamDamage = 2 };
            _registry = new FakeDamageRegistry();
            _ship = new Entity(new BodyPose());
        }

        private ShipRam Create() => new(_config, _registry);

        [Test]
        public void Construct_RegistersNothing()
        {
            Create();

            Assert.That(_registry.Sources, Is.Empty);
        }

        [Test]
        public void Attach_RegistersTheShipAsDamageSourceWithTheRamDamage()
        {
            Create().Attach(_ship);

            Assert.That(_registry.LiveSources, Is.EqualTo(1));
            Assert.That(_registry.Sources[0].Entity, Is.SameAs(_ship));
            Assert.That(_registry.Sources[0].Item.Damage, Is.EqualTo(2));
        }

        [Test]
        public void Dispose_RemovesTheDamageSourceRegistration()
        {
            Create().Attach(_ship).Dispose();

            Assert.That(_registry.LiveSources, Is.EqualTo(0));
        }

        [Test]
        public void Attach_Twice_Throws()
        {
            var ram = Create();
            ram.Attach(_ship);

            Assert.Throws<InvalidOperationException>(() => ram.Attach(_ship));
        }

        [Test]
        public void Attach_NullShip_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Create().Attach(null));
        }

        [Test]
        public void Construct_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new ShipRam(null, _registry));
            Assert.Throws<ArgumentNullException>(() => new ShipRam(_config, null));
        }
    }
}

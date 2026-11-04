using System;
using System.Collections.Generic;
using Asteroids.Core;
using NUnit.Framework;

namespace Asteroids.Weapons.Tests
{
    [TestFixture]
    public sealed class WeaponFactoriesTests
    {
        private class Definition : IWeaponDefinition
        {
            public string Name => "gun";
            public IReadOnlyList<Type> StatusContracts { get; set; } = Array.Empty<Type>();
        }

        private sealed class OtherDefinition : Definition { }

        private sealed class StubWeapon : IWeapon
        {
            public IDisposable Attach(IEntity carrier, IFireInput trigger) => throw new NotSupportedException();
        }

        private sealed class Factory<T> : WeaponFactory<T> where T : class, IWeaponDefinition
        {
            public List<T> Built { get; } = new();

            protected override IWeapon Create(T definition)
            {
                Built.Add(definition);
                return new StubWeapon();
            }
        }

        [Test]
        public void Create_UsesTheFactoryOfTheDefinitionsExactType()
        {
            var plain = new Factory<Definition>();
            var other = new Factory<OtherDefinition>();
            var factories = new WeaponFactories(new IWeaponFactory[] { plain, other });
            var definition = new OtherDefinition();

            var weapon = factories.Create(definition);

            Assert.That(weapon, Is.Not.Null);
            Assert.That(other.Built, Is.EqualTo(new[] { definition }));
            Assert.That(plain.Built, Is.Empty);
        }

        [Test]
        public void Create_NoFactoryForTheType_ThrowsNamingIt()
        {
            var factories = new WeaponFactories(new IWeaponFactory[] { new Factory<OtherDefinition>() });

            var error = Assert.Throws<InvalidOperationException>(() => factories.Create(new Definition()));
            Assert.That(error.Message, Does.Contain(nameof(Definition)));
        }

        [Test]
        public void Create_Null_Throws() =>
            Assert.Throws<ArgumentNullException>(() => new WeaponFactories(Array.Empty<IWeaponFactory>()).Create(null));

        [Test]
        public void Constructor_InvalidFactories_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new WeaponFactories(null));
            Assert.Throws<ArgumentException>(() => new WeaponFactories(new IWeaponFactory[] { null }));
            Assert.Throws<ArgumentException>(() =>
                new WeaponFactories(new IWeaponFactory[] { new Factory<Definition>(), new Factory<Definition>() }));
        }

        [Test]
        public void TypedFactory_WrongDefinitionType_Throws()
        {
            IWeaponFactory factory = new Factory<OtherDefinition>();

            Assert.That(factory.DefinitionType, Is.EqualTo(typeof(OtherDefinition)));
            Assert.Throws<ArgumentException>(() => factory.Create(new Definition()));
            Assert.Throws<ArgumentNullException>(() => factory.Create(null));
        }
    }

    [TestFixture]
    public sealed class WeaponStatusContractsTests
    {
        private interface IStatusA { }
        private interface IStatusB { }

        private sealed class Definition : IWeaponDefinition
        {
            public Definition(string name, params Type[] contracts)
            {
                Name = name;
                StatusContracts = contracts;
            }

            public string Name { get; }
            public IReadOnlyList<Type> StatusContracts { get; }
        }

        private sealed class StatusWeapon : IWeapon, IStatusA
        {
            public IDisposable Attach(IEntity carrier, IFireInput trigger) => throw new NotSupportedException();
        }

        private sealed class FakeLoadout : IShipLoadout
        {
            public IWeapon Primary { get; set; }

            public bool TryGetWeapon(WeaponSlot slot, out IWeapon weapon)
            {
                weapon = slot == WeaponSlot.Primary ? Primary : null;
                return weapon != null;
            }
        }

        [Test]
        public void Collect_EachContractWithItsSlot()
        {
            var contracts = WeaponStatusContracts.Collect(
                new Definition("a", typeof(IStatusA)), new Definition("b", typeof(IStatusB)));

            Assert.That(contracts, Is.EqualTo(new[]
            {
                (typeof(IStatusA), WeaponSlot.Primary), (typeof(IStatusB), WeaponSlot.Alternate)
            }));
        }

        [Test]
        public void Collect_EmptySlotsAndNoContracts_GiveNothing()
        {
            Assert.That(WeaponStatusContracts.Collect(null, null), Is.Empty);
            Assert.That(WeaponStatusContracts.Collect(new Definition("a"), null), Is.Empty);
        }

        [Test]
        public void Collect_TwoWeaponsWithTheSameContract_ThrowsNamingBoth()
        {
            var error = Assert.Throws<InvalidOperationException>(() => WeaponStatusContracts.Collect(
                new Definition("laser1", typeof(IStatusA)), new Definition("laser2", typeof(IStatusA))));

            Assert.That(error.Message, Does.Contain("laser1").And.Contain("laser2"));
        }

        [Test]
        public void Collect_ContractThatIsNotAnInterface_Throws() =>
            Assert.Throws<InvalidOperationException>(() =>
                WeaponStatusContracts.Collect(new Definition("a", typeof(string)), null));

        [Test]
        public void Resolve_ReturnsTheWeaponOfTheSlot()
        {
            var weapon = new StatusWeapon();

            var resolved = WeaponStatusContracts.Resolve(
                new FakeLoadout { Primary = weapon }, WeaponSlot.Primary, typeof(IStatusA));

            Assert.That(resolved, Is.SameAs(weapon));
        }

        [Test]
        public void Resolve_EmptySlotOrMissingContract_Throws()
        {
            var loadout = new FakeLoadout { Primary = new StatusWeapon() };

            Assert.Throws<InvalidOperationException>(() =>
                WeaponStatusContracts.Resolve(loadout, WeaponSlot.Alternate, typeof(IStatusA)));
            Assert.Throws<InvalidOperationException>(() =>
                WeaponStatusContracts.Resolve(loadout, WeaponSlot.Primary, typeof(IStatusB)));
            Assert.Throws<ArgumentNullException>(() =>
                WeaponStatusContracts.Resolve(null, WeaponSlot.Primary, typeof(IStatusA)));
        }
    }
}

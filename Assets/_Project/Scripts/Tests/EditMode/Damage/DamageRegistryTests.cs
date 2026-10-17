using System;
using NUnit.Framework;

namespace Asteroids.Damage.Tests
{
    [TestFixture]
    public sealed class DamageRegistryTests
    {
        private DamageRegistry _registry;
        private TestEntity _entity;

        [SetUp]
        public void SetUp()
        {
            _registry = new DamageRegistry();
            _entity = new TestEntity();
        }

        [Test]
        public void RegisterDamageable_MakesItAvailableByEntity()
        {
            var damageable = new RecordingDamageable();

            _registry.RegisterDamageable(_entity, damageable);

            Assert.That(_registry.TryGetDamageable(_entity, out var found), Is.True);
            Assert.That(found, Is.SameAs(damageable));
        }

        [Test]
        public void RegisterSource_MakesItAvailableByEntity()
        {
            var source = new TestSource(2);

            _registry.RegisterSource(_entity, source);

            Assert.That(_registry.TryGetSource(_entity, out var found), Is.True);
            Assert.That(found, Is.SameAs(source));
        }

        [Test]
        public void Lookup_UnknownEntity_ReturnsFalse()
        {
            Assert.That(_registry.TryGetDamageable(_entity, out _), Is.False);
            Assert.That(_registry.TryGetSource(_entity, out _), Is.False);
        }

        [Test]
        public void Registrations_AreIndependent_SoAnEntityCanBeBoth()
        {
            _registry.RegisterDamageable(_entity, new RecordingDamageable());

            Assert.That(_registry.TryGetSource(_entity, out _), Is.False);

            _registry.RegisterSource(_entity, new TestSource(1));

            Assert.That(_registry.TryGetDamageable(_entity, out _), Is.True);
            Assert.That(_registry.TryGetSource(_entity, out _), Is.True);
        }

        [Test]
        public void Lookup_DifferentEntity_ReturnsFalse()
        {
            _registry.RegisterDamageable(_entity, new RecordingDamageable());

            Assert.That(_registry.TryGetDamageable(new TestEntity(), out _), Is.False);
        }

        [Test]
        public void RegisterDamageable_SameEntityTwice_Throws()
        {
            _registry.RegisterDamageable(_entity, new RecordingDamageable());

            Assert.Throws<InvalidOperationException>(
                () => _registry.RegisterDamageable(_entity, new RecordingDamageable()));
        }

        [Test]
        public void RegisterSource_SameEntityTwice_Throws()
        {
            _registry.RegisterSource(_entity, new TestSource(1));

            Assert.Throws<InvalidOperationException>(() => _registry.RegisterSource(_entity, new TestSource(1)));
        }

        [Test]
        public void Dispose_RemovesTheRegistration()
        {
            var registration = _registry.RegisterDamageable(_entity, new RecordingDamageable());

            registration.Dispose();

            Assert.That(_registry.TryGetDamageable(_entity, out _), Is.False);
        }

        [Test]
        public void Dispose_Twice_DoesNotThrow()
        {
            var registration = _registry.RegisterSource(_entity, new TestSource(1));
            registration.Dispose();

            Assert.DoesNotThrow(() => registration.Dispose());
        }

        [Test]
        public void Dispose_AfterEntityWasRegisteredAgain_KeepsTheNewRegistration()
        {
            var first = _registry.RegisterDamageable(_entity, new RecordingDamageable());
            first.Dispose();
            var newer = new RecordingDamageable();
            _registry.RegisterDamageable(_entity, newer);

            first.Dispose();

            Assert.That(_registry.TryGetDamageable(_entity, out var found), Is.True);
            Assert.That(found, Is.SameAs(newer));
        }

        [Test]
        public void Register_AfterDispose_IsAllowed()
        {
            _registry.RegisterSource(_entity, new TestSource(1)).Dispose();

            Assert.DoesNotThrow(() => _registry.RegisterSource(_entity, new TestSource(2)));
        }

        [Test]
        public void Register_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(
                () => _registry.RegisterDamageable(null, new RecordingDamageable()));
            Assert.Throws<ArgumentNullException>(() => _registry.RegisterDamageable(_entity, null));
            Assert.Throws<ArgumentNullException>(() => _registry.RegisterSource(null, new TestSource(1)));
            Assert.Throws<ArgumentNullException>(() => _registry.RegisterSource(_entity, null));
        }
    }
}

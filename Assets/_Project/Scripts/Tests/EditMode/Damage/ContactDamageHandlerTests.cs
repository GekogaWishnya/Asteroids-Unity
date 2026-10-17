using System;
using Asteroids.Collisions;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Damage.Tests
{
    [TestFixture]
    public sealed class ContactDamageHandlerTests
    {
        private DamageRegistry _registry;
        private ContactDamageHandler _handler;
        private TestEntity _victim;
        private TestEntity _attacker;
        private RecordingDamageable _victimHealth;

        [SetUp]
        public void SetUp()
        {
            _registry = new DamageRegistry();
            _handler = new ContactDamageHandler(_registry);
            _victim = new TestEntity();
            _attacker = new TestEntity();
            _victimHealth = new RecordingDamageable();
        }

        private static CollisionEntered Entered(object selfOwner, object otherOwner) =>
            new(new TestCollider(selfOwner), new TestCollider(otherOwner), new Contact(Vector2.up, 1f));

        [Test]
        public void Entered_DamageableTouchesSource_TakesTheSourceDamage()
        {
            _registry.RegisterDamageable(_victim, _victimHealth);
            _registry.RegisterSource(_attacker, new TestSource(2));

            _handler.OnCollisionEntered(Entered(_victim, _attacker));

            Assert.That(_victimHealth.Hits, Is.EqualTo(new[] { 2 }));
        }

        [Test]
        public void Entered_ReportedFromTheSourceSide_DoesNotHurtTheSource()
        {
            _registry.RegisterDamageable(_victim, _victimHealth);
            _registry.RegisterSource(_attacker, new TestSource(2));

            _handler.OnCollisionEntered(Entered(_attacker, _victim));

            Assert.That(_victimHealth.Hits, Is.Empty);
        }

        [Test]
        public void Entered_BothAreDamageableAndSources_HurtEachOtherOncePerSide()
        {
            var attackerHealth = new RecordingDamageable();
            _registry.RegisterDamageable(_victim, _victimHealth);
            _registry.RegisterSource(_victim, new TestSource(3));
            _registry.RegisterDamageable(_attacker, attackerHealth);
            _registry.RegisterSource(_attacker, new TestSource(2));

            _handler.OnCollisionEntered(Entered(_victim, _attacker));
            _handler.OnCollisionEntered(Entered(_attacker, _victim));

            Assert.That(_victimHealth.Hits, Is.EqualTo(new[] { 2 }));
            Assert.That(attackerHealth.Hits, Is.EqualTo(new[] { 3 }));
        }

        [Test]
        public void Entered_SelfIsNotRegisteredAsDamageable_DoesNothing()
        {
            _registry.RegisterSource(_attacker, new TestSource(2));

            Assert.DoesNotThrow(() => _handler.OnCollisionEntered(Entered(_victim, _attacker)));
            Assert.That(_victimHealth.Hits, Is.Empty);
        }

        [Test]
        public void Entered_OtherIsNotRegisteredAsSource_DoesNothing()
        {
            _registry.RegisterDamageable(_victim, _victimHealth);

            _handler.OnCollisionEntered(Entered(_victim, _attacker));

            Assert.That(_victimHealth.Hits, Is.Empty);
        }

        [Test]
        public void Entered_SelfOwnerIsNotAnEntity_DoesNothing()
        {
            _registry.RegisterDamageable(_victim, _victimHealth);
            _registry.RegisterSource(_attacker, new TestSource(2));

            Assert.DoesNotThrow(() => _handler.OnCollisionEntered(Entered("not an entity", _attacker)));
            Assert.That(_victimHealth.Hits, Is.Empty);
        }

        [Test]
        public void Entered_OtherOwnerIsNotAnEntity_DoesNothing()
        {
            _registry.RegisterDamageable(_victim, _victimHealth);

            Assert.DoesNotThrow(() => _handler.OnCollisionEntered(Entered(_victim, "not an entity")));
            Assert.That(_victimHealth.Hits, Is.Empty);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Entered_SourceWithNonPositiveDamage_DealsNothing(int damage)
        {
            _registry.RegisterDamageable(_victim, _victimHealth);
            _registry.RegisterSource(_attacker, new TestSource(damage));

            Assert.DoesNotThrow(() => _handler.OnCollisionEntered(Entered(_victim, _attacker)));
            Assert.That(_victimHealth.Hits, Is.Empty);
        }

        [Test]
        public void Entered_SourceDamageIsReadAtContactTime()
        {
            var source = new TestSource(1);
            _registry.RegisterDamageable(_victim, _victimHealth);
            _registry.RegisterSource(_attacker, source);

            _handler.OnCollisionEntered(Entered(_victim, _attacker));
            source.Damage = 4;
            _handler.OnCollisionEntered(Entered(_victim, _attacker));

            Assert.That(_victimHealth.Hits, Is.EqualTo(new[] { 1, 4 }));
        }

        [Test]
        public void Entered_AfterDamageableRegistrationWasDisposed_DoesNothing()
        {
            var registration = _registry.RegisterDamageable(_victim, _victimHealth);
            _registry.RegisterSource(_attacker, new TestSource(2));
            registration.Dispose();

            _handler.OnCollisionEntered(Entered(_victim, _attacker));

            Assert.That(_victimHealth.Hits, Is.Empty);
        }

        [Test]
        public void Constructor_NullLookup_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ContactDamageHandler(null));
        }
    }
}

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
        private TestCollider _victimCollider;
        private TestCollider _attackerCollider;
        private RecordingDamageable _victimHealth;

        [SetUp]
        public void SetUp()
        {
            _registry = new DamageRegistry();
            _handler = new ContactDamageHandler(_registry);
            _victim = new TestEntity();
            _attacker = new TestEntity();
            _victimCollider = new TestCollider(_victim);
            _attackerCollider = new TestCollider(_attacker);
            _victimHealth = new RecordingDamageable();
        }

        /// <summary>Both signals of a new contact, in the order the collision system sends them.</summary>
        private void Touch(ICollider first, ICollider second) =>
            _handler.Entered(first, second, new Contact(Vector2.up, 1f));

        private void Separate(ICollider first, ICollider second) => _handler.Exited(first, second);

        [Test]
        public void Touch_DamageableTouchesSource_TakesTheSourceDamageOnce()
        {
            _registry.RegisterDamageable(_victim, _victimHealth);
            _registry.RegisterSource(_attacker, new TestSource(2));

            Touch(_victimCollider, _attackerCollider);

            Assert.That(_victimHealth.Hits, Is.EqualTo(new[] { 2 }));
        }

        [Test]
        public void Entered_ReportedFromTheSourceSide_StillHurtsTheVictim()
        {
            _registry.RegisterDamageable(_victim, _victimHealth);
            _registry.RegisterSource(_attacker, new TestSource(2));

            Touch(_attackerCollider, _victimCollider);

            Assert.That(_victimHealth.Hits, Is.EqualTo(new[] { 2 }));
        }

        [Test]
        public void Touch_BothAreDamageableAndSources_HurtEachOtherOnce()
        {
            var attackerHealth = new RecordingDamageable();
            _registry.RegisterDamageable(_victim, _victimHealth);
            _registry.RegisterSource(_victim, new TestSource(3));
            _registry.RegisterDamageable(_attacker, attackerHealth);
            _registry.RegisterSource(_attacker, new TestSource(2));

            Touch(_victimCollider, _attackerCollider);

            Assert.That(_victimHealth.Hits, Is.EqualTo(new[] { 2 }));
            Assert.That(attackerHealth.Hits, Is.EqualTo(new[] { 3 }));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Touch_FirstVictimDiesAndLeavesTheRegistry_StillDealsItsDamage(bool victimReportedFirst)
        {
            // A lethal hit takes the dead entity out of play, which removes its source registration.
            IDisposable victimSource = null;
            var victimHealth = new CallbackDamageable(() => victimSource.Dispose());
            var attackerHealth = new RecordingDamageable();
            _registry.RegisterDamageable(_victim, victimHealth);
            victimSource = _registry.RegisterSource(_victim, new TestSource(3));
            _registry.RegisterDamageable(_attacker, attackerHealth);
            _registry.RegisterSource(_attacker, new TestSource(2));

            if (victimReportedFirst) Touch(_victimCollider, _attackerCollider);
            else Touch(_attackerCollider, _victimCollider);

            Assert.That(victimHealth.Hits, Is.EqualTo(new[] { 2 }));
            Assert.That(attackerHealth.Hits, Is.EqualTo(new[] { 3 }));
        }

        [Test]
        public void Touch_FirstHitThrows_TheOtherStillLands()
        {
            var attackerHealth = new RecordingDamageable();
            _registry.RegisterDamageable(_victim, new CallbackDamageable(() => throw new InvalidOperationException()));
            _registry.RegisterSource(_victim, new TestSource(3));
            _registry.RegisterDamageable(_attacker, attackerHealth);
            _registry.RegisterSource(_attacker, new TestSource(2));

            Assert.Throws<InvalidOperationException>(() => Touch(_victimCollider, _attackerCollider));
            Assert.That(attackerHealth.Hits, Is.EqualTo(new[] { 3 }));
        }

        [Test]
        public void Stayed_DoesNothing()
        {
            _registry.RegisterDamageable(_victim, _victimHealth);
            _registry.RegisterSource(_attacker, new TestSource(2));

            _handler.Stayed(_victimCollider, _attackerCollider, new Contact(Vector2.up, 1f));

            Assert.That(_victimHealth.Hits, Is.Empty);
        }

        [Test]
        public void Touch_TwoCollidersOfTheSameEntity_DoesNothing()
        {
            _registry.RegisterDamageable(_victim, _victimHealth);
            _registry.RegisterSource(_victim, new TestSource(2));

            Touch(_victimCollider, new TestCollider(_victim));

            Assert.That(_victimHealth.Hits, Is.Empty);
        }

        [Test]
        public void Touch_SingleUseSource_HurtsAndIsSpent()
        {
            var source = new TestSingleUseSource(2);
            _registry.RegisterDamageable(_victim, _victimHealth);
            _registry.RegisterSource(_attacker, source);

            Touch(_victimCollider, _attackerCollider);

            Assert.That(_victimHealth.Hits, Is.EqualTo(new[] { 2 }));
            Assert.That(source.SpendCalls, Is.EqualTo(1));
        }

        [Test]
        public void Touch_SingleUseSourceTouchesTwoVictims_OnlyTheFirstIsHurt()
        {
            var secondVictim = new TestEntity();
            var secondHealth = new RecordingDamageable();
            _registry.RegisterDamageable(_victim, _victimHealth);
            _registry.RegisterDamageable(secondVictim, secondHealth);
            _registry.RegisterSource(_attacker, new TestSingleUseSource(1));

            Touch(_victimCollider, _attackerCollider);
            Touch(_attackerCollider, new TestCollider(secondVictim));

            Assert.That(_victimHealth.Hits, Is.EqualTo(new[] { 1 }));
            Assert.That(secondHealth.Hits, Is.Empty);
        }

        [Test]
        public void Touch_SourceThatIsNotSingleUseTouchesTwoVictims_HurtsBoth()
        {
            var secondVictim = new TestEntity();
            var secondHealth = new RecordingDamageable();
            _registry.RegisterDamageable(_victim, _victimHealth);
            _registry.RegisterDamageable(secondVictim, secondHealth);
            _registry.RegisterSource(_attacker, new TestSource(1));

            Touch(_victimCollider, _attackerCollider);
            Touch(_attackerCollider, new TestCollider(secondVictim));

            Assert.That(_victimHealth.Hits, Is.EqualTo(new[] { 1 }));
            Assert.That(secondHealth.Hits, Is.EqualTo(new[] { 1 }));
        }

        [Test]
        public void Touch_SpentSingleUseSource_DealsNothingAndIsNotSpentAgain()
        {
            var source = new TestSingleUseSource(2) { IsSpent = true };
            _registry.RegisterDamageable(_victim, _victimHealth);
            _registry.RegisterSource(_attacker, source);

            Touch(_victimCollider, _attackerCollider);

            Assert.That(_victimHealth.Hits, Is.Empty);
            Assert.That(source.SpendCalls, Is.Zero);
        }

        [Test]
        public void Touch_SingleUseSourceAndNothingToHurt_IsNotSpent()
        {
            var source = new TestSingleUseSource(2);
            _registry.RegisterSource(_attacker, source);

            Touch(_victimCollider, _attackerCollider);

            Assert.That(source.SpendCalls, Is.Zero);
        }

        [Test]
        public void Touch_SingleUseSourceWhoseVictimThrows_IsStillSpent()
        {
            var source = new TestSingleUseSource(2);
            _registry.RegisterDamageable(_victim, new CallbackDamageable(() => throw new InvalidOperationException()));
            _registry.RegisterSource(_attacker, source);

            Assert.Throws<InvalidOperationException>(() => Touch(_victimCollider, _attackerCollider));
            Assert.That(source.IsSpent, Is.True);
        }

        [Test]
        public void Order_IsDamage()
        {
            Assert.That(_handler.Order, Is.EqualTo(CollisionResponseOrder.Damage));
        }

        [Test]
        public void Touch_AgainAfterSeparating_HurtsAgain()
        {
            _registry.RegisterDamageable(_victim, _victimHealth);
            _registry.RegisterSource(_attacker, new TestSource(2));

            Touch(_victimCollider, _attackerCollider);
            Separate(_victimCollider, _attackerCollider);
            Touch(_victimCollider, _attackerCollider);

            Assert.That(_victimHealth.Hits, Is.EqualTo(new[] { 2, 2 }));
        }

        [Test]
        public void Touch_SelfIsNotRegisteredAsDamageable_DoesNothing()
        {
            _registry.RegisterSource(_attacker, new TestSource(2));

            Assert.DoesNotThrow(() => Touch(_victimCollider, _attackerCollider));
            Assert.That(_victimHealth.Hits, Is.Empty);
        }

        [Test]
        public void Touch_OtherIsNotRegisteredAsSource_DoesNothing()
        {
            _registry.RegisterDamageable(_victim, _victimHealth);

            Touch(_victimCollider, _attackerCollider);

            Assert.That(_victimHealth.Hits, Is.Empty);
        }

        [Test]
        public void Touch_OwnerIsNotAnEntity_DoesNothing()
        {
            _registry.RegisterDamageable(_victim, _victimHealth);
            _registry.RegisterSource(_attacker, new TestSource(2));

            Assert.DoesNotThrow(() => Touch(new TestCollider("not an entity"), _attackerCollider));
            Assert.DoesNotThrow(() => Touch(_victimCollider, new TestCollider("not an entity")));
            Assert.That(_victimHealth.Hits, Is.Empty);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Touch_SourceWithNonPositiveDamage_DealsNothing(int damage)
        {
            _registry.RegisterDamageable(_victim, _victimHealth);
            _registry.RegisterSource(_attacker, new TestSource(damage));

            Assert.DoesNotThrow(() => Touch(_victimCollider, _attackerCollider));
            Assert.That(_victimHealth.Hits, Is.Empty);
        }

        [Test]
        public void Touch_SourceDamageIsReadAtContactTime()
        {
            var source = new TestSource(1);
            _registry.RegisterDamageable(_victim, _victimHealth);
            _registry.RegisterSource(_attacker, source);

            Touch(_victimCollider, _attackerCollider);
            Separate(_victimCollider, _attackerCollider);
            source.Damage = 4;
            Touch(_victimCollider, _attackerCollider);

            Assert.That(_victimHealth.Hits, Is.EqualTo(new[] { 1, 4 }));
        }

        [Test]
        public void Touch_AfterDamageableRegistrationWasDisposed_DoesNothing()
        {
            var registration = _registry.RegisterDamageable(_victim, _victimHealth);
            _registry.RegisterSource(_attacker, new TestSource(2));
            registration.Dispose();

            Touch(_victimCollider, _attackerCollider);

            Assert.That(_victimHealth.Hits, Is.Empty);
        }

        [Test]
        public void Constructor_NullLookup_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ContactDamageHandler(null));
        }
    }
}

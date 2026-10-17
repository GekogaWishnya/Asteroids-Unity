using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Asteroids.Health.Tests
{
    [TestFixture]
    public sealed class HealthModelTests
    {
        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_NonPositiveMax_Throws(int max)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HealthModel(max));
        }

        [Test]
        public void Constructor_StartsFullAndAlive()
        {
            var health = new HealthModel(3);

            Assert.That(health.Current.Value, Is.EqualTo(3));
            Assert.That(health.Max.Value, Is.EqualTo(3));
            Assert.That(health.IsDead, Is.False);
        }

        [Test]
        public void TakeDamage_LowersCurrentAndKeepsMax()
        {
            var health = new HealthModel(3);

            health.TakeDamage(1);

            Assert.That(health.Current.Value, Is.EqualTo(2));
            Assert.That(health.Max.Value, Is.EqualTo(3));
            Assert.That(health.IsDead, Is.False);
        }

        [Test]
        public void TakeDamage_MoreThanRemaining_ClampsAtZeroAndKills()
        {
            var health = new HealthModel(3);

            health.TakeDamage(10);

            Assert.That(health.Current.Value, Is.EqualTo(0));
            Assert.That(health.IsDead, Is.True);
        }

        [TestCase(0)]
        [TestCase(-5)]
        public void TakeDamage_NonPositiveAmount_Throws(int amount)
        {
            var health = new HealthModel(3);

            Assert.Throws<ArgumentOutOfRangeException>(() => health.TakeDamage(amount));
            Assert.That(health.Current.Value, Is.EqualTo(3));
        }

        [Test]
        public void TakeDamage_NotLethal_DoesNotRaiseDepleted()
        {
            var health = new HealthModel(3);
            var raised = 0;
            health.Depleted += () => raised++;

            health.TakeDamage(2);

            Assert.That(raised, Is.EqualTo(0));
        }

        [Test]
        public void TakeDamage_Lethal_RaisesDepletedOnceAfterCurrentIsZero()
        {
            var health = new HealthModel(3);
            var raised = 0;
            var currentWhenRaised = -1;
            health.Depleted += () =>
            {
                raised++;
                currentWhenRaised = health.Current.Value;
            };

            health.TakeDamage(3);

            Assert.That(raised, Is.EqualTo(1));
            Assert.That(currentWhenRaised, Is.EqualTo(0));
        }

        [Test]
        public void TakeDamage_WhenDead_IsIgnoredAndDoesNotRaiseDepletedAgain()
        {
            var health = new HealthModel(1);
            var raised = 0;
            health.Depleted += () => raised++;
            health.TakeDamage(1);

            health.TakeDamage(1);

            Assert.That(health.Current.Value, Is.EqualTo(0));
            Assert.That(raised, Is.EqualTo(1));
        }

        [Test]
        public void TakeDamage_ObserverOfCurrentThrows_StillRaisesDepleted()
        {
            var health = new HealthModel(1);
            var raised = 0;
            health.Depleted += () => raised++;
            health.Current.Subscribe(value =>
            {
                if (value == 0) throw new InvalidOperationException();
            });

            Assert.Throws<InvalidOperationException>(() => health.TakeDamage(1));

            Assert.That(raised, Is.EqualTo(1));
            Assert.That(health.IsDead, Is.True);
        }

        [Test]
        public void TakeDamage_NotifiesObserversOfCurrent()
        {
            var health = new HealthModel(3);
            var seen = new List<int>();
            health.Current.Subscribe(seen.Add);

            health.TakeDamage(1);
            health.TakeDamage(2);

            Assert.That(seen, Is.EqualTo(new[] { 3, 2, 0 }));
        }

        [Test]
        public void Heal_RaisesCurrent()
        {
            var health = new HealthModel(5);
            health.TakeDamage(3);

            health.Heal(2);

            Assert.That(health.Current.Value, Is.EqualTo(4));
        }

        [Test]
        public void Heal_AboveMax_ClampsAtMax()
        {
            var health = new HealthModel(5);
            health.TakeDamage(1);

            health.Heal(int.MaxValue);

            Assert.That(health.Current.Value, Is.EqualTo(5));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Heal_NonPositiveAmount_Throws(int amount)
        {
            var health = new HealthModel(5);

            Assert.Throws<ArgumentOutOfRangeException>(() => health.Heal(amount));
        }

        [Test]
        public void Heal_WhenDead_IsIgnored()
        {
            var health = new HealthModel(3);
            health.TakeDamage(3);

            health.Heal(2);

            Assert.That(health.Current.Value, Is.EqualTo(0));
            Assert.That(health.IsDead, Is.True);
        }

        [Test]
        public void Reset_AfterDamage_RefillsHealth()
        {
            var health = new HealthModel(3);
            health.TakeDamage(2);

            health.Reset();

            Assert.That(health.Current.Value, Is.EqualTo(3));
        }

        [Test]
        public void Reset_WhenDead_RevivesAndLetsDepletedFireAgain()
        {
            var health = new HealthModel(2);
            var raised = 0;
            health.Depleted += () => raised++;
            health.TakeDamage(2);

            health.Reset();
            health.TakeDamage(2);

            Assert.That(health.IsDead, Is.True);
            Assert.That(raised, Is.EqualTo(2));
        }

        [Test]
        public void Reset_WhenAlreadyFull_DoesNotNotify()
        {
            var health = new HealthModel(3);
            var seen = new List<int>();
            health.Current.Subscribe(seen.Add);
            seen.Clear();

            health.Reset();

            Assert.That(seen, Is.Empty);
        }
    }
}

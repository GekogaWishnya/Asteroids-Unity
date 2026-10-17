using System;
using NUnit.Framework;

namespace Asteroids.Player.Tests
{
    [TestFixture]
    public sealed class PlayerHealthTests
    {
        private static PlayerHealth Create(int maxHealth = 3) =>
            new(new TestHealthConfig { MaxHealth = maxHealth });

        [Test]
        public void Constructor_UsesMaxHealthOfTheConfigForCurrentAndMax()
        {
            var health = Create(5);

            Assert.That(health.Current.Value, Is.EqualTo(5));
            Assert.That(health.Max.Value, Is.EqualTo(5));
            Assert.That(health.IsDead, Is.False);
        }

        [Test]
        public void Constructor_NullConfig_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new PlayerHealth(null));
        }

        [Test]
        public void Constructor_NonPositiveMaxHealth_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Create(0));
        }

        [Test]
        public void TakeDamage_LowersCurrent()
        {
            var health = Create();

            health.TakeDamage(2);

            Assert.That(health.Current.Value, Is.EqualTo(1));
        }

        [Test]
        public void Heal_RaisesCurrentUpToMax()
        {
            var health = Create();
            health.TakeDamage(2);

            health.Heal(10);

            Assert.That(health.Current.Value, Is.EqualTo(3));
        }

        [Test]
        public void Reset_RevivesTheDeadPlayer()
        {
            var health = Create();
            health.TakeDamage(3);

            health.Reset();

            Assert.That(health.IsDead, Is.False);
            Assert.That(health.Current.Value, Is.EqualTo(3));
        }

        [Test]
        public void Depleted_IsForwardedToSubscribersAndCanBeUnsubscribed()
        {
            var health = Create(1);
            var raised = 0;
            Action handler = () => raised++;
            health.Depleted += handler;

            health.TakeDamage(1);
            health.Reset();
            health.Depleted -= handler;
            health.TakeDamage(1);

            Assert.That(raised, Is.EqualTo(1));
        }
    }
}

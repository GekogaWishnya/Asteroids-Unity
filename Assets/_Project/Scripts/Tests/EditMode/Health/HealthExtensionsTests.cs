using System;
using System.Collections.Generic;
using Asteroids.Core;
using NUnit.Framework;

namespace Asteroids.Health.Tests
{
    [TestFixture]
    public sealed class HealthExtensionsTests
    {
        [Test]
        public void SubscribeDepleted_HealthRunsOut_CallsTheHandler()
        {
            var health = new HealthModel(1);
            var calls = 0;
            health.SubscribeDepleted(() => calls++);

            health.TakeDamage(1);

            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void SubscribeDepleted_Disposed_NoLongerCalls()
        {
            var health = new HealthModel(1);
            var calls = 0;
            var subscription = health.SubscribeDepleted(() => calls++);

            subscription.Dispose();
            subscription.Dispose();
            health.TakeDamage(1);

            Assert.That(calls, Is.Zero);
        }

        [Test]
        public void SubscribeDepleted_NullArgument_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => HealthExtensions.SubscribeDepleted(null, () => { }));
            Assert.Throws<ArgumentNullException>(() => new HealthModel(1).SubscribeDepleted(null));
        }

        [Test]
        public void SubscribeDropped_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => HealthExtensions.SubscribeDropped(null, _ => { }));
            Assert.Throws<ArgumentNullException>(() => new HealthModel(1).SubscribeDropped(null));
        }

        [Test]
        public void SubscribeDropped_DoesNotReportTheStartingValue()
        {
            var drops = new List<bool>();

            new HealthModel(3).SubscribeDropped(drops.Add);

            Assert.That(drops, Is.Empty);
        }

        [Test]
        public void Damage_ReportsEachDropAndWhetherItKilled()
        {
            var health = new HealthModel(3);
            var drops = new List<bool>();
            health.SubscribeDropped(drops.Add);

            health.TakeDamage(1);
            health.TakeDamage(5);

            Assert.That(drops, Is.EqualTo(new[] { false, true }));
        }

        [Test]
        public void FatalDrop_IsReportedBeforeDepleted()
        {
            var health = new HealthModel(1);
            var log = new List<string>();
            health.Depleted += () => log.Add("depleted");
            health.SubscribeDropped(fatal => log.Add(fatal ? "fatal drop" : "drop"));

            health.TakeDamage(1);

            Assert.That(log, Is.EqualTo(new[] { "fatal drop", "depleted" }));
        }

        [Test]
        public void HealAndReset_AreNotReported()
        {
            var health = new HealthModel(3);
            var drops = new List<bool>();
            health.SubscribeDropped(drops.Add);
            health.TakeDamage(2);

            health.Heal(1);
            health.TakeDamage(5);
            health.Reset();

            Assert.That(drops, Is.EqualTo(new[] { false, true }));
        }

        [Test]
        public void DamageAfterDeath_IsNotReported()
        {
            var health = new HealthModel(1);
            var drops = new List<bool>();
            health.SubscribeDropped(drops.Add);
            health.TakeDamage(1);

            health.TakeDamage(1);

            Assert.That(drops, Has.Count.EqualTo(1));
        }

        [Test]
        public void Dispose_StopsReporting()
        {
            var health = new HealthModel(3);
            var drops = new List<bool>();

            health.SubscribeDropped(drops.Add).Dispose();
            health.TakeDamage(1);

            Assert.That(drops, Is.Empty);
        }
    }
}

using System;
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
    }
}

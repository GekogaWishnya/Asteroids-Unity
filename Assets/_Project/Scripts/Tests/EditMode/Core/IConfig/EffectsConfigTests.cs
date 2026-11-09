using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Asteroids.Core.IConfigs.Tests
{
    [TestFixture]
    public sealed class EffectsConfigTests
    {
        private sealed class Feedback : IEnemyFeedbackConfig
        {
            public Feedback(string type) => Type = new EnemyTypeId(type);

            public EnemyTypeId Type { get; }
            public IEffectConfig Hit => null;
            public bool PlayHitOnFatal => false;
            public IEffectConfig Death => null;
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new EffectsConfig(null, new List<IEnemyFeedbackConfig>()));
            Assert.Throws<ArgumentNullException>(() => new EffectsConfig(new Feedback("Ship"), null));
        }

        [Test]
        public void Constructor_KeepsTheShipAndTheEnemiesInOrder()
        {
            var ship = new Feedback("Ship");
            var a = new Feedback("A");
            var b = new Feedback("B");

            var config = new EffectsConfig(ship, new[] { a, b });

            Assert.That(config.Ship, Is.SameAs(ship));
            Assert.That(config.Enemies, Is.EqualTo(new IEnemyFeedbackConfig[] { a, b }));
        }

        [Test]
        public void Constructor_CopiesTheEnemies_SoLaterChangesToTheSourceDoNotLeakIn()
        {
            var source = new List<IEnemyFeedbackConfig> { new Feedback("A") };

            var config = new EffectsConfig(new Feedback("Ship"), source);
            source.Add(new Feedback("B"));

            Assert.That(config.Enemies, Has.Count.EqualTo(1));
        }
    }
}

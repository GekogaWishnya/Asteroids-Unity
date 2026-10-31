using System;
using Asteroids.Common;
using Asteroids.Core.IConfigs;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Enemies.Tests
{
    [TestFixture]
    public sealed class AsteroidLauncherTests
    {
        private static AsteroidPopulation PopulationOf(string id) =>
            new(
                new TestAsteroidType(id),
                () => new AsteroidController(
                    new FakeEnemyView(),
                    new TestAsteroidType(id),
                    new FakeSimulationLoop(),
                    new FakeDamageRegistry(),
                    new FakeKineticBodyRegistry(),
                    new RecordingDeathHandler()),
                new SystemRandom(5));

        [Test]
        public void Register_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new AsteroidLauncher().Register(null));
        }

        [Test]
        public void Register_SameTypeTwice_ThrowsNamingTheType()
        {
            var launcher = new AsteroidLauncher();
            launcher.Register(PopulationOf("AsteroidBig"));

            var ex = Assert.Throws<InvalidOperationException>(() => launcher.Register(PopulationOf("AsteroidBig")));

            StringAssert.Contains("AsteroidBig", ex.Message);
        }

        [Test]
        public void Launch_RoutesToThePopulationOfTheType()
        {
            var launcher = new AsteroidLauncher();
            var big = PopulationOf("AsteroidBig");
            var medium = PopulationOf("AsteroidMedium");
            launcher.Register(big);
            launcher.Register(medium);

            launcher.Launch(new EnemyTypeId("AsteroidMedium"), Vector2.zero);
            launcher.Launch(new EnemyTypeId("AsteroidMedium"), Vector2.zero);

            Assert.That(big.AliveCount, Is.EqualTo(0));
            Assert.That(medium.AliveCount, Is.EqualTo(2));
        }

        [Test]
        public void Launch_UnknownType_ThrowsNamingTheType()
        {
            var launcher = new AsteroidLauncher();
            launcher.Register(PopulationOf("AsteroidBig"));

            var ex = Assert.Throws<InvalidOperationException>(() =>
                launcher.Launch(new EnemyTypeId("Ghost"), Vector2.zero));

            StringAssert.Contains("Ghost", ex.Message);
        }

        [Test]
        public void Launch_DefaultType_Throws()
        {
            var launcher = new AsteroidLauncher();

            Assert.Throws<InvalidOperationException>(() => launcher.Launch(default, Vector2.zero));
        }
    }
}

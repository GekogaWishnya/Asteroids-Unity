using System;
using Asteroids.Common;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Enemies.Tests
{
    [TestFixture]
    public sealed class AsteroidSpawnerBuilderTests
    {
        private sealed class Rig
        {
            public readonly FakeEnemyViewFactory Views = new();
            public readonly FakeSimulationLoop Loop = new();
            public readonly FakeDamageRegistry Damage = new();
            public readonly FakeKineticBodyRegistry Bodies = new();
            public readonly FakeScoreSink Score = new();
            public readonly AsteroidSpawnerBuilder Builder;

            public Rig(TestAsteroidConfig config) =>
                Builder = new AsteroidSpawnerBuilder(
                    config,
                    Views,
                    Loop,
                    Damage,
                    Bodies,
                    Score,
                    new TestWorldBounds(new Rect(-8f, -8f, 16f, 16f)),
                    new SystemRandom(9));

            public int TotalAlive
            {
                get
                {
                    var total = 0;
                    foreach (var spawner in Builder.Spawners) total += spawner.AliveCount;
                    return total;
                }
            }
        }

        private static TestAsteroidConfig ThreeSizes() => new(
            new TestAsteroidType("AsteroidBig") { ScorePoints = 20 }.SplitsInto("AsteroidMedium", 2),
            new TestAsteroidType("AsteroidMedium") { ScorePoints = 50 }.SplitsInto("AsteroidSmall", 2),
            new TestAsteroidType("AsteroidSmall") { ScorePoints = 100 });

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            var config = ThreeSizes();
            var views = new FakeEnemyViewFactory();
            var loop = new FakeSimulationLoop();
            var damage = new FakeDamageRegistry();
            var bodies = new FakeKineticBodyRegistry();
            var score = new FakeScoreSink();
            var bounds = new TestWorldBounds(new Rect(-8f, -8f, 16f, 16f));
            var random = new SystemRandom(1);

            Assert.Throws<ArgumentNullException>(() =>
                new AsteroidSpawnerBuilder(null, views, loop, damage, bodies, score, bounds, random));
            Assert.Throws<ArgumentNullException>(() =>
                new AsteroidSpawnerBuilder(config, null, loop, damage, bodies, score, bounds, random));
            Assert.Throws<ArgumentNullException>(() =>
                new AsteroidSpawnerBuilder(config, views, null, damage, bodies, score, bounds, random));
            Assert.Throws<ArgumentNullException>(() =>
                new AsteroidSpawnerBuilder(config, views, loop, null, bodies, score, bounds, random));
            Assert.Throws<ArgumentNullException>(() =>
                new AsteroidSpawnerBuilder(config, views, loop, damage, null, score, bounds, random));
            Assert.Throws<ArgumentNullException>(() =>
                new AsteroidSpawnerBuilder(config, views, loop, damage, bodies, null, bounds, random));
            Assert.Throws<ArgumentNullException>(() =>
                new AsteroidSpawnerBuilder(config, views, loop, damage, bodies, score, null, random));
            Assert.Throws<ArgumentNullException>(() =>
                new AsteroidSpawnerBuilder(config, views, loop, damage, bodies, score, bounds, null));
        }

        [Test]
        public void Spawners_OnePerConfiguredType()
        {
            var rig = new Rig(ThreeSizes());

            var spawners = rig.Builder.Spawners;

            Assert.That(spawners, Has.Count.EqualTo(3));
            Assert.That(spawners[0].Type.Value, Is.EqualTo("AsteroidBig"));
            Assert.That(spawners[1].Type.Value, Is.EqualTo("AsteroidMedium"));
            Assert.That(spawners[2].Type.Value, Is.EqualTo("AsteroidSmall"));
        }

        [Test]
        public void Spawners_AreBuiltOnce()
        {
            var config = new TestAsteroidConfig(new TestAsteroidType("A") { PoolPrewarm = 2 });
            var rig = new Rig(config);

            var first = rig.Builder.Spawners;
            var second = rig.Builder.Spawners;

            Assert.That(second, Is.SameAs(first));
            Assert.That(rig.Views.Views, Has.Count.EqualTo(2));
        }

        [Test]
        public void Spawners_PrewarmThePoolsOfEveryType()
        {
            var config = new TestAsteroidConfig(
                new TestAsteroidType("A") { PoolPrewarm = 2 },
                new TestAsteroidType("B") { PoolPrewarm = 3 });
            var rig = new Rig(config);

            Assert.That(rig.Builder.Spawners, Has.Count.EqualTo(2));
            Assert.That(rig.Views.Views, Has.Count.EqualTo(5));
            Assert.That(rig.Views.Types.FindAll(t => t.Value == "A"), Has.Count.EqualTo(2));
            Assert.That(rig.Views.Types.FindAll(t => t.Value == "B"), Has.Count.EqualTo(3));
            Assert.That(rig.Views.ShownCount, Is.EqualTo(0));
        }

        [Test]
        public void Spawners_InvalidConfig_ThrowsListingTheProblems()
        {
            var config = new TestAsteroidConfig(
                new TestAsteroidType("A").SplitsInto("B", 2),
                new TestAsteroidType("B") { MaxHealth = 0 }.SplitsInto("A", 2));
            var rig = new Rig(config);

            var ex = Assert.Throws<InvalidOperationException>(() => { _ = rig.Builder.Spawners; });

            StringAssert.Contains("Split cycle", ex.Message);
            StringAssert.Contains("max health", ex.Message);
            Assert.That(rig.Views.Views, Is.Empty);
        }

        [Test]
        public void Spawn_Big_SplitsDownTheWholeChainAndScoresEveryStep()
        {
            var rig = new Rig(ThreeSizes());
            rig.Builder.Spawners[0].Spawn();
            Assert.That(rig.TotalAlive, Is.EqualTo(1));

            rig.Damage.HitAll();

            Assert.That(rig.Builder.Spawners[0].AliveCount, Is.EqualTo(0));
            Assert.That(rig.Builder.Spawners[1].AliveCount, Is.EqualTo(2));
            Assert.That(rig.TotalAlive, Is.EqualTo(2));
            Assert.That(rig.Score.Awarded, Is.EqualTo(new[] { 20 }));

            rig.Damage.HitAll();

            Assert.That(rig.Builder.Spawners[2].AliveCount, Is.EqualTo(4));
            Assert.That(rig.TotalAlive, Is.EqualTo(4));
            Assert.That(rig.Score.Awarded, Is.EqualTo(new[] { 20, 50, 50 }));

            rig.Damage.HitAll();

            Assert.That(rig.TotalAlive, Is.EqualTo(0));
            Assert.That(rig.Score.Awarded, Has.Count.EqualTo(7));
            Assert.That(rig.Views.ShownCount, Is.EqualTo(0));
        }

        [Test]
        public void Spawn_TypeWithoutPointsOrChildren_DiesQuietly()
        {
            var rig = new Rig(new TestAsteroidConfig(new TestAsteroidType("A") { ScorePoints = 0 }));
            rig.Builder.Spawners[0].Spawn();

            rig.Damage.HitAll();

            Assert.That(rig.Score.Awarded, Is.Empty);
            Assert.That(rig.TotalAlive, Is.EqualTo(0));
            Assert.That(rig.Views.ShownCount, Is.EqualTo(0));
        }

        [Test]
        public void Spawn_AfterEverythingDied_ReusesTheObjects()
        {
            var rig = new Rig(ThreeSizes());
            rig.Builder.Spawners[0].Spawn();
            rig.Damage.HitAll();
            rig.Damage.HitAll();
            rig.Damage.HitAll();
            var created = rig.Views.Views.Count;

            rig.Builder.Spawners[0].Spawn();
            rig.Damage.HitAll();
            rig.Damage.HitAll();
            rig.Damage.HitAll();

            Assert.That(rig.Views.Views, Has.Count.EqualTo(created));
            Assert.That(rig.TotalAlive, Is.EqualTo(0));
        }
    }
}

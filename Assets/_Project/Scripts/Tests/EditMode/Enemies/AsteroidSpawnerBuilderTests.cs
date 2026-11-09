using System;
using Asteroids.Common;
using Asteroids.Core.IConfigs;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Enemies.Tests
{
    [TestFixture]
    public sealed class AsteroidSpawnerBuilderTests
    {
        private static readonly Rect Area = new(-8f, -8f, 16f, 16f);

        private sealed class Rig
        {
            public readonly FakeEnemyViewFactory Views = new();
            public readonly FakeSimulationLoop Loop = new();
            public readonly FakeWrapRegistry Wrap = new();
            public readonly FakeDamageRegistry Damage = new();
            public readonly FakeKineticBodyRegistry Bodies = new();
            public readonly FakeScoreSink Score = new();
            public readonly TestSpawnEntryConfig Entry = new() { SpawnMargin = 1f };
            public readonly RecordingEnemyEvents Events = new();
            public readonly AsteroidSpawnerBuilder Builder;

            public Rig(TestAsteroidConfig config, IEnemyEventSink events = null) =>
                Builder = new AsteroidSpawnerBuilder(
                    config,
                    Views,
                    Loop,
                    Wrap,
                    Damage,
                    Bodies,
                    Score,
                    events ?? Events,
                    new TestWorldBounds(Area),
                    new SystemRandom(9),
                    Entry);

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

        private sealed class ThrowingEnemyEvents : IEnemyEventSink
        {
            public void Damaged(EnemyTypeId type, Vector2 position, bool isFatal) { }

            public void Died(EnemyTypeId type, Vector2 position) =>
                throw new InvalidOperationException("listener failed");
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
            var wrap = new FakeWrapRegistry();
            var damage = new FakeDamageRegistry();
            var bodies = new FakeKineticBodyRegistry();
            var score = new FakeScoreSink();
            var events = new RecordingEnemyEvents();
            var bounds = new TestWorldBounds(new Rect(-8f, -8f, 16f, 16f));
            var random = new SystemRandom(1);
            var entry = new TestSpawnEntryConfig();

            Assert.Throws<ArgumentNullException>(() =>
                new AsteroidSpawnerBuilder(null, views, loop, wrap, damage, bodies, score, events, bounds, random, entry));
            Assert.Throws<ArgumentNullException>(() =>
                new AsteroidSpawnerBuilder(config, null, loop, wrap, damage, bodies, score, events, bounds, random, entry));
            Assert.Throws<ArgumentNullException>(() =>
                new AsteroidSpawnerBuilder(config, views, null, wrap, damage, bodies, score, events, bounds, random, entry));
            Assert.Throws<ArgumentNullException>(() =>
                new AsteroidSpawnerBuilder(config, views, loop, null, damage, bodies, score, events, bounds, random, entry));
            Assert.Throws<ArgumentNullException>(() =>
                new AsteroidSpawnerBuilder(config, views, loop, wrap, null, bodies, score, events, bounds, random, entry));
            Assert.Throws<ArgumentNullException>(() =>
                new AsteroidSpawnerBuilder(config, views, loop, wrap, damage, null, score, events, bounds, random, entry));
            Assert.Throws<ArgumentNullException>(() =>
                new AsteroidSpawnerBuilder(config, views, loop, wrap, damage, bodies, null, events, bounds, random, entry));
            Assert.Throws<ArgumentNullException>(() =>
                new AsteroidSpawnerBuilder(config, views, loop, wrap, damage, bodies, score, null, bounds, random, entry));
            Assert.Throws<ArgumentNullException>(() =>
                new AsteroidSpawnerBuilder(config, views, loop, wrap, damage, bodies, score, events, null, random, entry));
            Assert.Throws<ArgumentNullException>(() =>
                new AsteroidSpawnerBuilder(config, views, loop, wrap, damage, bodies, score, events, bounds, null, entry));
            Assert.Throws<ArgumentNullException>(() =>
                new AsteroidSpawnerBuilder(config, views, loop, wrap, damage, bodies, score, events, bounds, random, null));
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
        public void Spawn_Killed_ReportsTheFatalHitAndThenTheDeathWithTheType()
        {
            var rig = new Rig(new TestAsteroidConfig(new TestAsteroidType("A") { MaxHealth = 2 }));
            rig.Builder.Spawners[0].Spawn();

            rig.Damage.HitAll();
            rig.Damage.HitAll();

            Assert.That(rig.Events.Damaged, Has.Count.EqualTo(2));
            Assert.That(rig.Events.Damaged[0].Type.Value, Is.EqualTo("A"));
            Assert.That(rig.Events.Damaged[0].IsFatal, Is.False);
            Assert.That(rig.Events.Damaged[1].IsFatal, Is.True);
            Assert.That(rig.Events.Died, Has.Count.EqualTo(1));
            Assert.That(rig.Events.Died[0].Type.Value, Is.EqualTo("A"));
            Assert.That(rig.Events.Died[0].Position, Is.EqualTo(rig.Events.Damaged[1].Position));
        }

        [Test]
        public void Spawn_ReportFails_ScoreAndSplitsStillHappen()
        {
            var rig = new Rig(ThreeSizes(), new ThrowingEnemyEvents());
            rig.Builder.Spawners[0].Spawn();

            Assert.Throws<InvalidOperationException>(() => rig.Damage.HitAll());

            Assert.That(rig.Score.Awarded, Is.EqualTo(new[] { 20 }));
            Assert.That(rig.Builder.Spawners[1].AliveCount, Is.EqualTo(2));
            Assert.That(rig.Builder.Spawners[0].AliveCount, Is.EqualTo(0));
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

        [Test]
        public void Spawn_StartsOutsideTheAreaByTheMargin()
        {
            var rig = new Rig(new TestAsteroidConfig(new TestAsteroidType("A") { PoolMaxIdle = 100 }));

            for (var i = 0; i < 100; i++) rig.Builder.Spawners[0].Spawn();

            // PerimeterSpawnPoints insets the right and top edges by 0.001, so a start there is 0.001 short of
            // the full margin. The tolerance must be well above that, or float rounding decides the result.
            const float margin = 1f, epsilon = 1e-2f;
            foreach (var view in rig.Views.Views)
            {
                var p = view.Entity.Pose.Position;
                var outside = p.x <= Area.xMin - margin + epsilon
                              || p.x >= Area.xMax + margin - epsilon
                              || p.y <= Area.yMin - margin + epsilon
                              || p.y >= Area.yMax + margin - epsilon;
                Assert.That(outside, Is.True, $"({p.x}, {p.y}) starts less than the margin outside the area");
            }
        }

        [Test]
        public void Spawners_InvalidEntryConfig_Throws()
        {
            var rig = new Rig(ThreeSizes());
            rig.Entry.SpawnMargin = -1f;

            var ex = Assert.Throws<InvalidOperationException>(() => { _ = rig.Builder.Spawners; });

            StringAssert.Contains("Spawn margin", ex.Message);
        }
    }
}

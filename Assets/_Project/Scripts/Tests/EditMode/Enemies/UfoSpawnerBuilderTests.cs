using System;
using Asteroids.Common;
using Asteroids.Physics;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Enemies.Tests
{
    [TestFixture]
    public sealed class UfoSpawnerBuilderTests
    {
        private sealed class Rig
        {
            public readonly FakeEnemyViewFactory Views = new() { CreateEntity = () => new Entity(new BodyPose()) };
            public readonly FakePlayerTarget Target = FakePlayerTarget.At(0f, 0f);
            public readonly FakeSimulationLoop Loop = new();
            public readonly FakeWrapRegistry Wrap = new();
            public readonly FakeDamageRegistry Damage = new();
            public readonly FakeKineticBodyRegistry Bodies = new();
            public readonly FakeScoreSink Score = new();
            public readonly TestWorldBounds Bounds = new(new Rect(-8f, -8f, 16f, 16f));
            public readonly SystemRandom Random = new(5);
            public readonly TestSpawnEntryConfig Entry = new() { SpawnMargin = 1f };
            public readonly UfoSpawnerBuilder Builder;

            public Rig(TestUfoConfig config) =>
                Builder = new UfoSpawnerBuilder(
                    config, Views, Target, Loop, Wrap, Damage, Bodies, Score, Bounds, Random, Entry);
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            var rig = new Rig(new TestUfoConfig(new TestUfoType()));
            var config = new TestUfoConfig(new TestUfoType());

            Assert.Throws<ArgumentNullException>(() => new UfoSpawnerBuilder(
                null, rig.Views, rig.Target, rig.Loop, rig.Wrap, rig.Damage, rig.Bodies, rig.Score, rig.Bounds, rig.Random, rig.Entry));
            Assert.Throws<ArgumentNullException>(() => new UfoSpawnerBuilder(
                config, null, rig.Target, rig.Loop, rig.Wrap, rig.Damage, rig.Bodies, rig.Score, rig.Bounds, rig.Random, rig.Entry));
            Assert.Throws<ArgumentNullException>(() => new UfoSpawnerBuilder(
                config, rig.Views, null, rig.Loop, rig.Wrap, rig.Damage, rig.Bodies, rig.Score, rig.Bounds, rig.Random, rig.Entry));
            Assert.Throws<ArgumentNullException>(() => new UfoSpawnerBuilder(
                config, rig.Views, rig.Target, null, rig.Wrap, rig.Damage, rig.Bodies, rig.Score, rig.Bounds, rig.Random, rig.Entry));
            Assert.Throws<ArgumentNullException>(() => new UfoSpawnerBuilder(
                config, rig.Views, rig.Target, rig.Loop, null, rig.Damage, rig.Bodies, rig.Score, rig.Bounds, rig.Random, rig.Entry));
            Assert.Throws<ArgumentNullException>(() => new UfoSpawnerBuilder(
                config, rig.Views, rig.Target, rig.Loop, rig.Wrap, null, rig.Bodies, rig.Score, rig.Bounds, rig.Random, rig.Entry));
            Assert.Throws<ArgumentNullException>(() => new UfoSpawnerBuilder(
                config, rig.Views, rig.Target, rig.Loop, rig.Wrap, rig.Damage, null, rig.Score, rig.Bounds, rig.Random, rig.Entry));
            Assert.Throws<ArgumentNullException>(() => new UfoSpawnerBuilder(
                config, rig.Views, rig.Target, rig.Loop, rig.Wrap, rig.Damage, rig.Bodies, null, rig.Bounds, rig.Random, rig.Entry));
            Assert.Throws<ArgumentNullException>(() => new UfoSpawnerBuilder(
                config, rig.Views, rig.Target, rig.Loop, rig.Wrap, rig.Damage, rig.Bodies, rig.Score, null, rig.Random, rig.Entry));
            Assert.Throws<ArgumentNullException>(() => new UfoSpawnerBuilder(
                config, rig.Views, rig.Target, rig.Loop, rig.Wrap, rig.Damage, rig.Bodies, rig.Score, rig.Bounds, null, rig.Entry));
            Assert.Throws<ArgumentNullException>(() => new UfoSpawnerBuilder(
                config, rig.Views, rig.Target, rig.Loop, rig.Wrap, rig.Damage, rig.Bodies, rig.Score, rig.Bounds, rig.Random, null));
        }

        [Test]
        public void Spawners_InvalidConfig_ThrowsWithEveryProblem()
        {
            var rig = new Rig(new TestUfoConfig(new TestUfoType { MaxHealth = 0, LaunchSpeed = -1f }));

            var error = Assert.Throws<InvalidOperationException>(() => _ = rig.Builder.Spawners);
            StringAssert.Contains("2 problem(s)", error.Message);
        }

        [Test]
        public void Spawners_OnePerTypeBuiltOnceAndPrewarmed()
        {
            var rig = new Rig(new TestUfoConfig(
                new TestUfoType("Ufo") { PoolPrewarm = 2 },
                new TestUfoType("UfoFast") { PoolPrewarm = 1 }));

            var spawners = rig.Builder.Spawners;

            Assert.That(spawners, Has.Count.EqualTo(2));
            Assert.That(spawners[0].Type.Value, Is.EqualTo("Ufo"));
            Assert.That(spawners[1].Type.Value, Is.EqualTo("UfoFast"));
            Assert.That(rig.Builder.Spawners, Is.SameAs(spawners));
            Assert.That(rig.Views.Views, Has.Count.EqualTo(3));
        }

        [Test]
        public void Death_AwardsTheTypesPoints()
        {
            var rig = new Rig(new TestUfoConfig(new TestUfoType { ScorePoints = 150 }));
            rig.Builder.Spawners[0].Spawn();

            rig.Damage.HitLatest();

            Assert.That(rig.Score.Awarded, Is.EqualTo(new[] { 150 }));
            Assert.That(rig.Builder.Spawners[0].AliveCount, Is.EqualTo(0));
        }

        [Test]
        public void Death_WithoutPoints_AwardsNothing()
        {
            var rig = new Rig(new TestUfoConfig(new TestUfoType { ScorePoints = 0 }));
            rig.Builder.Spawners[0].Spawn();

            rig.Damage.HitLatest();

            Assert.That(rig.Score.Awarded, Is.Empty);
        }

        [Test]
        public void Spawn_StartsOutsideTheAreaByTheMargin()
        {
            var rig = new Rig(new TestUfoConfig(new TestUfoType { PoolMaxIdle = 100 }));

            for (var i = 0; i < 100; i++) rig.Builder.Spawners[0].Spawn();

            // PerimeterSpawnPoints insets the right and top edges by 0.001, so a start there is 0.001 short of
            // the full margin. The tolerance must be well above that, or float rounding decides the result.
            const float margin = 1f, epsilon = 1e-2f;
            var area = rig.Bounds.Rect;
            foreach (var view in rig.Views.Views)
            {
                var p = view.Entity.Pose.Position;
                var outside = p.x <= area.xMin - margin + epsilon
                              || p.x >= area.xMax + margin - epsilon
                              || p.y <= area.yMin - margin + epsilon
                              || p.y >= area.yMax + margin - epsilon;
                Assert.That(outside, Is.True, $"({p.x}, {p.y}) starts less than the margin outside the area");
            }
        }

        [Test]
        public void Spawners_InvalidEntryConfig_Throws()
        {
            var rig = new Rig(new TestUfoConfig(new TestUfoType()));
            rig.Entry.MaxEnteringSeconds = 0f;

            var error = Assert.Throws<InvalidOperationException>(() => _ = rig.Builder.Spawners);

            StringAssert.Contains("Max entering seconds", error.Message);
        }
    }
}

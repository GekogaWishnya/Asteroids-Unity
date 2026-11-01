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
            public readonly FakeDamageRegistry Damage = new();
            public readonly FakeKineticBodyRegistry Bodies = new();
            public readonly FakeScoreSink Score = new();
            public readonly TestWorldBounds Bounds = new(new Rect(-8f, -8f, 16f, 16f));
            public readonly SystemRandom Random = new(5);
            public readonly UfoSpawnerBuilder Builder;

            public Rig(TestUfoConfig config) =>
                Builder = new UfoSpawnerBuilder(config, Views, Target, Loop, Damage, Bodies, Score, Bounds, Random);
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            var rig = new Rig(new TestUfoConfig(new TestUfoType()));
            var config = new TestUfoConfig(new TestUfoType());

            Assert.Throws<ArgumentNullException>(() => new UfoSpawnerBuilder(
                null, rig.Views, rig.Target, rig.Loop, rig.Damage, rig.Bodies, rig.Score, rig.Bounds, rig.Random));
            Assert.Throws<ArgumentNullException>(() => new UfoSpawnerBuilder(
                config, null, rig.Target, rig.Loop, rig.Damage, rig.Bodies, rig.Score, rig.Bounds, rig.Random));
            Assert.Throws<ArgumentNullException>(() => new UfoSpawnerBuilder(
                config, rig.Views, null, rig.Loop, rig.Damage, rig.Bodies, rig.Score, rig.Bounds, rig.Random));
            Assert.Throws<ArgumentNullException>(() => new UfoSpawnerBuilder(
                config, rig.Views, rig.Target, null, rig.Damage, rig.Bodies, rig.Score, rig.Bounds, rig.Random));
            Assert.Throws<ArgumentNullException>(() => new UfoSpawnerBuilder(
                config, rig.Views, rig.Target, rig.Loop, null, rig.Bodies, rig.Score, rig.Bounds, rig.Random));
            Assert.Throws<ArgumentNullException>(() => new UfoSpawnerBuilder(
                config, rig.Views, rig.Target, rig.Loop, rig.Damage, null, rig.Score, rig.Bounds, rig.Random));
            Assert.Throws<ArgumentNullException>(() => new UfoSpawnerBuilder(
                config, rig.Views, rig.Target, rig.Loop, rig.Damage, rig.Bodies, null, rig.Bounds, rig.Random));
            Assert.Throws<ArgumentNullException>(() => new UfoSpawnerBuilder(
                config, rig.Views, rig.Target, rig.Loop, rig.Damage, rig.Bodies, rig.Score, null, rig.Random));
            Assert.Throws<ArgumentNullException>(() => new UfoSpawnerBuilder(
                config, rig.Views, rig.Target, rig.Loop, rig.Damage, rig.Bodies, rig.Score, rig.Bounds, null));
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
    }
}

using System;
using Asteroids.Common;
using Asteroids.Physics;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Enemies.Tests
{
    [TestFixture]
    public sealed class UfoSpawnerTests
    {
        private const float Tolerance = 1e-3f;
        private static readonly Rect Area = new(-8f, -8f, 16f, 16f);

        /// <summary>No distance outside the edge, so enemies start on the edge itself.</summary>
        private static readonly OffscreenEntry OnTheEdge = new(0f);

        private sealed class Rig
        {
            public readonly FakeEnemyViewFactory Views = new() { CreateEntity = () => new Entity(new BodyPose()) };
            public readonly FakeSimulationLoop Loop = new();
            public readonly FakeWrapRegistry Wrap = new();
            public readonly FakeDamageRegistry Damage = new();
            public readonly FakeKineticBodyRegistry Bodies = new();
            public readonly FakePlayerTarget Target = FakePlayerTarget.At(0f, 0f);
            public readonly TestUfoType Config = new() { LaunchSpeed = 2f, PoolPrewarm = 2, PoolMaxIdle = 4 };
            public readonly PerimeterSpawnPoints Perimeter;
            public readonly UfoSpawner Spawner;

            public Rig(IRandom random, OffscreenEntry entry = null)
            {
                Perimeter = new PerimeterSpawnPoints(new TestWorldBounds(Area), random);
                Spawner = new UfoSpawner(Config, CreateController, Perimeter, Target, entry ?? OnTheEdge);
            }

            public UfoController CreateController() =>
                new(Views.Create(Config.Type), Config, Target, Loop, Wrap, Damage, Bodies,
                    new RecordingDamageHandler(), new RecordingDeathHandler());

            public Vector2 LatestPosition => Views.Views[Views.Views.Count - 1].Entity.Pose.Position;
            public Vector2 LatestVelocity => Bodies.Registrations[Bodies.Registrations.Count - 1].Body.Velocity;
            public void KillLatest() => Damage.HitLatest(Config.MaxHealth);
        }

        private static void AssertVector(Vector2 actual, float x, float y)
        {
            Assert.That(actual.x, Is.EqualTo(x).Within(Tolerance));
            Assert.That(actual.y, Is.EqualTo(y).Within(Tolerance));
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            var rig = new Rig(new SystemRandom(1));

            Assert.Throws<ArgumentNullException>(() =>
                new UfoSpawner(null, rig.CreateController, rig.Perimeter, rig.Target, OnTheEdge));
            Assert.Throws<ArgumentNullException>(() =>
                new UfoSpawner(rig.Config, null, rig.Perimeter, rig.Target, OnTheEdge));
            Assert.Throws<ArgumentNullException>(() =>
                new UfoSpawner(rig.Config, rig.CreateController, null, rig.Target, OnTheEdge));
            Assert.Throws<ArgumentNullException>(() =>
                new UfoSpawner(rig.Config, rig.CreateController, rig.Perimeter, null, OnTheEdge));
            Assert.Throws<ArgumentNullException>(() =>
                new UfoSpawner(rig.Config, rig.CreateController, rig.Perimeter, rig.Target, null));
        }

        [Test]
        public void Type_IsTheConfigsType()
        {
            var rig = new Rig(new SystemRandom(1));

            Assert.That(rig.Spawner.Type, Is.EqualTo(rig.Config.Type));
        }

        [Test]
        public void Prewarm_CreatesTheConfiguredIdleUfos()
        {
            var rig = new Rig(new SystemRandom(1));

            rig.Spawner.Prewarm();

            Assert.That(rig.Views.Views, Has.Count.EqualTo(2));
            Assert.That(rig.Views.ShownCount, Is.EqualTo(0));
            Assert.That(rig.Spawner.AliveCount, Is.EqualTo(0));
        }

        [Test]
        public void Spawn_WithAShip_StartsOnTheEdgeAimedAtTheShip()
        {
            // Bottom edge, a quarter of the way along: (-4, -8).
            var rig = new Rig(new ScriptedRandom(4f / 64f));
            rig.Target.Position = new Vector2(2f, 0f);

            rig.Spawner.Spawn();

            AssertVector(rig.LatestPosition, -4f, -8f);
            AssertVector(rig.LatestVelocity, 1.2f, 1.6f);
        }

        [Test]
        public void Spawn_WithoutAShip_EntersStraightIntoTheAreaWhenNotTilted()
        {
            var rig = new Rig(new ScriptedRandom(0.125f, 0.5f));
            rig.Target.HasTarget = false;

            rig.Spawner.Spawn();

            AssertVector(rig.LatestPosition, 0f, -8f);
            AssertVector(rig.LatestVelocity, 0f, 2f);
        }

        [Test]
        public void Spawn_ShipOnTheSpawnPoint_FallsBackToAnInwardHeading()
        {
            var rig = new Rig(new ScriptedRandom(0.125f, 0.5f));
            rig.Target.Position = new Vector2(0f, -8f);

            rig.Spawner.Spawn();

            AssertVector(rig.LatestVelocity, 0f, 2f);
        }

        [Test]
        public void Spawn_CountsAliveAndDeathCountsDown()
        {
            var rig = new Rig(new SystemRandom(2));

            rig.Spawner.Spawn();
            rig.Spawner.Spawn();
            rig.KillLatest();

            Assert.That(rig.Spawner.AliveCount, Is.EqualTo(1));
        }

        [Test]
        public void DespawnAll_TakesEveryUfoOutOfPlay()
        {
            var rig = new Rig(new SystemRandom(2));
            rig.Spawner.Spawn();
            rig.Spawner.Spawn();

            rig.Spawner.DespawnAll();

            Assert.That(rig.Spawner.AliveCount, Is.Zero);
            Assert.That(rig.Views.ShownCount, Is.Zero);
            Assert.That(rig.Damage.LiveDamageables, Is.Zero);
        }

        [Test]
        public void Spawn_AfterADeath_ReusesThePooledUfo()
        {
            var rig = new Rig(new SystemRandom(3));
            rig.Spawner.Spawn();
            rig.KillLatest();

            rig.Spawner.Spawn();

            Assert.That(rig.Views.Views, Has.Count.EqualTo(1));
            Assert.That(rig.Views.ShownCount, Is.EqualTo(1));
            Assert.That(rig.Spawner.AliveCount, Is.EqualTo(1));
        }

        [Test]
        public void Spawn_WithAnEntryDistance_StartsOutsideOnTheLineToTheShip()
        {
            // Bottom edge at (-4, -8), aimed at (2, 0): along (0.6, 0.8), so a depth of 2 is 2.5 back.
            var rig = new Rig(new ScriptedRandom(4f / 64f), new OffscreenEntry(2f));
            rig.Target.Position = new Vector2(2f, 0f);

            rig.Spawner.Spawn();

            AssertVector(rig.LatestPosition, -5.5f, -10f);
            AssertVector(rig.LatestVelocity, 1.2f, 1.6f);
        }

        [Test]
        public void Spawn_RegistersTheUfoAsEntering()
        {
            var rig = new Rig(new SystemRandom(2));

            rig.Spawner.Spawn();

            Assert.That(rig.Wrap.Entering, Has.Count.EqualTo(1));
            Assert.That(rig.Wrap.Entering[0], Is.SameAs(rig.Views.Views[0].Entity.Pose));
        }
    }
}
using System;
using Asteroids.Common;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Enemies.Tests
{
    [TestFixture]
    public sealed class AsteroidSpawnerTests
    {
        private const float Speed = 2f;
        private const float Tolerance = 1e-3f;
        private static readonly Rect Area = new(-8f, -8f, 16f, 16f);

        private sealed class Rig
        {
            public readonly FakeEnemyViewFactory Views = new();
            public readonly FakeSimulationLoop Loop = new();
            public readonly AsteroidPopulation Population;
            public readonly AsteroidSpawner Spawner;

            public Rig(IRandom random)
            {
                var config = new TestAsteroidType("AsteroidBig")
                {
                    PoolMaxIdle = 1000, MinSpeed = Speed, MaxSpeed = Speed
                };
                var damage = new FakeDamageRegistry();

                Population = new AsteroidPopulation(
                    config,
                    () => new AsteroidController(
                        Views.Create(config.Type), config, Loop, damage, new RecordingDeathHandler()),
                    random);
                Spawner = new AsteroidSpawner(Population, new TestWorldBounds(Area), random);
            }

            public Vector2 FirstPosition => Views.Views[0].Entity.Pose.Position;

            /// <summary>Direction the first asteroid flies in, seen after one short tick.</summary>
            public Vector2 FirstHeading()
            {
                var before = FirstPosition;
                Loop.Tick(0.1f);
                return (FirstPosition - before).normalized;
            }
        }

        /// <summary>Distance from <paramref name="position"/> along <paramref name="direction"/> until the half-open area is left.</summary>
        private static float TravelBeforeLeaving(Vector2 position, Vector2 direction)
        {
            var travel = float.PositiveInfinity;

            if (direction.x > 1e-6f) travel = Mathf.Min(travel, (Area.xMax - position.x) / direction.x);
            else if (direction.x < -1e-6f) travel = Mathf.Min(travel, (Area.xMin - position.x) / direction.x);

            if (direction.y > 1e-6f) travel = Mathf.Min(travel, (Area.yMax - position.y) / direction.y);
            else if (direction.y < -1e-6f) travel = Mathf.Min(travel, (Area.yMin - position.y) / direction.y);

            return travel;
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            var rig = new Rig(new SystemRandom(1));
            var bounds = new TestWorldBounds(Area);
            var random = new SystemRandom(1);

            Assert.Throws<ArgumentNullException>(() => new AsteroidSpawner(null, bounds, random));
            Assert.Throws<ArgumentNullException>(() => new AsteroidSpawner(rig.Population, null, random));
            Assert.Throws<ArgumentNullException>(() => new AsteroidSpawner(rig.Population, bounds, null));
        }

        [TestCase(AsteroidSpawner.MinTravel, 10f)]
        [TestCase(10f, AsteroidSpawner.MinTravel)]
        public void Constructor_AreaNotLargerThanMinTravel_Throws(float width, float height)
        {
            var rig = new Rig(new SystemRandom(1));
            var bounds = new TestWorldBounds(new Rect(0f, 0f, width, height));

            Assert.Throws<ArgumentException>(
                () => new AsteroidSpawner(rig.Population, bounds, new SystemRandom(1)));
        }

        [Test]
        public void TypeAndAliveCount_FollowThePopulation()
        {
            var rig = new Rig(new SystemRandom(2));

            rig.Spawner.Spawn();

            Assert.That(rig.Spawner.Type, Is.EqualTo(rig.Population.Type));
            Assert.That(rig.Spawner.AliveCount, Is.EqualTo(1));
        }

        [Test]
        public void Spawn_FirstQuarterOfThePerimeter_IsOnTheBottomEdge()
        {
            var rig = new Rig(new ScriptedRandom(0.125f));

            rig.Spawner.Spawn();

            Assert.That(rig.FirstPosition.x, Is.EqualTo(0f).Within(1e-4f));
            Assert.That(rig.FirstPosition.y, Is.EqualTo(-8f).Within(1e-4f));
        }

        [Test]
        public void Spawn_SecondQuarter_IsOnTheRightEdge()
        {
            var rig = new Rig(new ScriptedRandom(0.375f));

            rig.Spawner.Spawn();

            Assert.That(rig.FirstPosition.x, Is.EqualTo(7.999f).Within(1e-4f));
            Assert.That(rig.FirstPosition.y, Is.EqualTo(0f).Within(1e-4f));
        }

        [Test]
        public void Spawn_ThirdQuarter_IsOnTheTopEdge()
        {
            var rig = new Rig(new ScriptedRandom(0.625f));

            rig.Spawner.Spawn();

            Assert.That(rig.FirstPosition.x, Is.EqualTo(0f).Within(1e-4f));
            Assert.That(rig.FirstPosition.y, Is.EqualTo(7.999f).Within(1e-4f));
        }

        [Test]
        public void Spawn_LastQuarter_IsOnTheLeftEdge()
        {
            var rig = new Rig(new ScriptedRandom(0.875f));

            rig.Spawner.Spawn();

            Assert.That(rig.FirstPosition.x, Is.EqualTo(-8f).Within(1e-4f));
            Assert.That(rig.FirstPosition.y, Is.EqualTo(0f).Within(1e-4f));
        }

        [Test]
        public void Spawn_Always_StaysInsideTheHalfOpenRectOnAnEdge()
        {
            var rig = new Rig(new SystemRandom(3));
            const int count = 500;

            for (var i = 0; i < count; i++) rig.Spawner.Spawn();

            for (var i = 0; i < count; i++)
            {
                var position = rig.Views.Views[i].Entity.Pose.Position;
                Assert.That(position.x, Is.GreaterThanOrEqualTo(Area.xMin).And.LessThan(Area.xMax));
                Assert.That(position.y, Is.GreaterThanOrEqualTo(Area.yMin).And.LessThan(Area.yMax));

                var onEdge = Mathf.Approximately(position.x, Area.xMin)
                             || position.x > Area.xMax - 0.01f
                             || Mathf.Approximately(position.y, Area.yMin)
                             || position.y > Area.yMax - 0.01f;
                Assert.That(onEdge, Is.True, $"({position.x}, {position.y}) is not on an edge");
            }
        }

        [TestCase(0.125f, 0f, 1f)]
        [TestCase(0.375f, -1f, 0f)]
        [TestCase(0.625f, 0f, -1f)]
        [TestCase(0.875f, 1f, 0f)]
        public void Spawn_MidEdgeWithoutTilt_HeadsStraightIntoTheArea(float along, float x, float y)
        {
            var rig = new Rig(new ScriptedRandom(along, 0.5f));

            rig.Spawner.Spawn();
            var heading = rig.FirstHeading();

            Assert.That(heading.x, Is.EqualTo(x).Within(Tolerance));
            Assert.That(heading.y, Is.EqualTo(y).Within(Tolerance));
        }

        [TestCase(0f, -1f)]
        [TestCase(1f, 1f)]
        public void Spawn_FullTilt_TurnsByTheMaximumTowardTheChosenEnd(float roll, float sideOfX)
        {
            // Bottom edge: the start is its left end and the end is its right end.
            var rig = new Rig(new ScriptedRandom(0.125f, roll));

            rig.Spawner.Spawn();
            var heading = rig.FirstHeading();

            Assert.That(
                Vector2.Angle(heading, Vector2.up),
                Is.EqualTo(AsteroidSpawner.MaxTiltFromInward).Within(0.01f));
            Assert.That(Mathf.Sign(heading.x), Is.EqualTo(sideOfX));
        }

        [Test]
        public void Spawn_HalfAUnitFromACorner_LeansTowardItOnlyAsFarAsMinTravelAllows()
        {
            // 15.5 along the bottom edge: 0.5 from its right end.
            var rig = new Rig(new ScriptedRandom(15.5f / 64f, 1f));

            rig.Spawner.Spawn();
            var start = rig.FirstPosition;
            var heading = rig.FirstHeading();

            Assert.That(heading.x, Is.GreaterThan(0f));
            Assert.That(
                TravelBeforeLeaving(start, heading),
                Is.EqualTo(AsteroidSpawner.MinTravel).Within(Tolerance));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void Spawn_Always_TravelsAtLeastMinTravelBeforeLeavingTheArea(int seed)
        {
            var rig = new Rig(new SystemRandom(seed));
            const int count = 700;
            for (var i = 0; i < count; i++) rig.Spawner.Spawn();

            var starts = new Vector2[count];
            for (var i = 0; i < count; i++) starts[i] = rig.Views.Views[i].Entity.Pose.Position;
            rig.Loop.Tick(0.1f);

            for (var i = 0; i < count; i++)
            {
                var heading = (rig.Views.Views[i].Entity.Pose.Position - starts[i]).normalized;
                Assert.That(
                    TravelBeforeLeaving(starts[i], heading),
                    Is.GreaterThanOrEqualTo(AsteroidSpawner.MinTravel - Tolerance),
                    $"spawn {i} at ({starts[i].x}, {starts[i].y}) heading ({heading.x}, {heading.y})");
            }
        }
    }
}

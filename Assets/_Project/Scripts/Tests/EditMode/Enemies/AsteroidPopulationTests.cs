using System;
using Asteroids.Common;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Enemies.Tests
{
    [TestFixture]
    public sealed class AsteroidPopulationTests
    {
        private sealed class Rig
        {
            public readonly FakeEnemyViewFactory Views = new();
            public readonly FakeSimulationLoop Loop = new();
            public readonly FakeDamageRegistry Damage = new();
            public readonly FakeKineticBodyRegistry Bodies = new();
            public readonly TestAsteroidType Config;
            public readonly AsteroidPopulation Population;

            public Rig(TestAsteroidType config = null, IRandom random = null)
            {
                Config = config ?? new TestAsteroidType("AsteroidBig");
                Population = new AsteroidPopulation(
                    Config,
                    () => new AsteroidController(
                        Views.Create(Config.Type), Config, Loop, Damage, Bodies, new RecordingDeathHandler()),
                    random ?? new SystemRandom(11));
            }
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            var config = new TestAsteroidType("A");

            Assert.Throws<ArgumentNullException>(() =>
                new AsteroidPopulation(null, () => null, new SystemRandom(1)));
            Assert.Throws<ArgumentNullException>(() => new AsteroidPopulation(config, null, new SystemRandom(1)));
            Assert.Throws<ArgumentNullException>(() => new AsteroidPopulation(config, () => null, null));
        }

        [Test]
        public void Type_IsTheTypeOfTheConfig()
        {
            var rig = new Rig(new TestAsteroidType("AsteroidMedium"));

            Assert.That(rig.Population.Type, Is.EqualTo(rig.Config.Type));
        }

        [Test]
        public void Launch_PutsAnAsteroidInPlayAtThePosition()
        {
            var rig = new Rig();

            rig.Population.Launch(new Vector2(3f, 4f));

            Assert.That(rig.Population.AliveCount, Is.EqualTo(1));
            Assert.That(rig.Views.Views, Has.Count.EqualTo(1));
            Assert.That(rig.Views.Views[0].IsShown, Is.True);
            Assert.That(rig.Views.Views[0].Entity.Pose.Position, Is.EqualTo(new Vector2(3f, 4f)));
        }

        [Test]
        public void Launch_RollsSpeedAndSpinWithinTheRangesOfTheType()
        {
            var rig = new Rig(new TestAsteroidType("A")
            {
                MinSpeed = 1f, MaxSpeed = 3f, MinSpinSpeed = 20f, MaxSpinSpeed = 60f, PoolMaxIdle = 100
            });
            const int count = 60;
            for (var i = 0; i < count; i++) rig.Population.Launch(Vector2.zero);

            var anglesBefore = new float[count];
            for (var i = 0; i < count; i++) anglesBefore[i] = rig.Views.Views[i].Entity.Pose.Angle;

            rig.Loop.Tick(1f);

            for (var i = 0; i < count; i++)
            {
                var pose = rig.Views.Views[i].Entity.Pose;
                Assert.That(pose.Position.magnitude, Is.InRange(1f - 1e-3f, 3f + 1e-3f));
                Assert.That(Mathf.Abs(pose.Angle - anglesBefore[i]), Is.InRange(20f - 1e-2f, 60f + 1e-2f));
            }
        }

        [Test]
        public void Launch_SpinGoesBothWays()
        {
            var rig = new Rig(new TestAsteroidType("A") { PoolMaxIdle = 100 });
            const int count = 60;
            for (var i = 0; i < count; i++) rig.Population.Launch(Vector2.zero);

            var before = new float[count];
            for (var i = 0; i < count; i++) before[i] = rig.Views.Views[i].Entity.Pose.Angle;
            rig.Loop.Tick(0.1f);

            var clockwise = 0;
            var counterClockwise = 0;
            for (var i = 0; i < count; i++)
            {
                if (rig.Views.Views[i].Entity.Pose.Angle > before[i]) counterClockwise++;
                else clockwise++;
            }

            Assert.That(clockwise, Is.GreaterThan(0));
            Assert.That(counterClockwise, Is.GreaterThan(0));
        }

        [Test]
        public void Launch_HeadingsDiffer()
        {
            var rig = new Rig(new TestAsteroidType("A") { PoolMaxIdle = 100 });
            for (var i = 0; i < 20; i++) rig.Population.Launch(Vector2.zero);

            rig.Loop.Tick(1f);

            var distinct = new System.Collections.Generic.HashSet<Vector2>();
            foreach (var view in rig.Views.Views) distinct.Add(view.Entity.Pose.Position);
            Assert.That(distinct.Count, Is.GreaterThan(1));
        }

        [Test]
        public void Finished_CountsDownAndTheInstanceIsReused()
        {
            var rig = new Rig();
            rig.Population.Launch(Vector2.zero);

            rig.Damage.HitAll();

            Assert.That(rig.Population.AliveCount, Is.EqualTo(0));

            rig.Population.Launch(Vector2.one);

            Assert.That(rig.Views.Views, Has.Count.EqualTo(1));
            Assert.That(rig.Population.AliveCount, Is.EqualTo(1));
            Assert.That(rig.Views.Views[0].IsShown, Is.True);
        }

        [Test]
        public void Launch_ManyAtOnce_NeverRefuses()
        {
            var rig = new Rig(new TestAsteroidType("A") { PoolMaxIdle = 1 });

            for (var i = 0; i < 25; i++) rig.Population.Launch(Vector2.zero);

            Assert.That(rig.Population.AliveCount, Is.EqualTo(25));
            Assert.That(rig.Views.ShownCount, Is.EqualTo(25));
        }

        [Test]
        public void Prewarm_CreatesIdleAsteroidsWithoutPuttingThemInPlay()
        {
            var rig = new Rig(new TestAsteroidType("A") { PoolPrewarm = 3 });

            rig.Population.Prewarm();

            Assert.That(rig.Views.Views, Has.Count.EqualTo(3));
            Assert.That(rig.Views.ShownCount, Is.EqualTo(0));
            Assert.That(rig.Population.AliveCount, Is.EqualTo(0));
        }

        [Test]
        public void Launch_AfterPrewarm_UsesAPrewarmedAsteroid()
        {
            var rig = new Rig(new TestAsteroidType("A") { PoolPrewarm = 2 });
            rig.Population.Prewarm();

            rig.Population.Launch(Vector2.zero);

            Assert.That(rig.Views.Views, Has.Count.EqualTo(2));
            Assert.That(rig.Views.ShownCount, Is.EqualTo(1));
        }

        [Test]
        public void Finished_AboveMaxIdle_DestroysTheSurplus()
        {
            var rig = new Rig(new TestAsteroidType("A") { PoolMaxIdle = 1 });
            for (var i = 0; i < 3; i++) rig.Population.Launch(Vector2.zero);

            rig.Damage.HitAll();

            Assert.That(rig.Population.AliveCount, Is.EqualTo(0));
            Assert.That(rig.Views.DisposedCount, Is.EqualTo(2));
        }

        [Test]
        public void Launch_StartFails_KeepsTheCountAndPoolsTheAsteroidAgain()
        {
            var rig = new Rig();
            rig.Population.Launch(Vector2.zero);
            rig.Damage.HitAll();
            rig.Views.Views[0].FailOnShow = true;

            Assert.Throws<InvalidOperationException>(() => rig.Population.Launch(Vector2.zero));

            Assert.That(rig.Population.AliveCount, Is.EqualTo(0));

            rig.Views.Views[0].FailOnShow = false;
            rig.Population.Launch(Vector2.zero);

            Assert.That(rig.Views.Views, Has.Count.EqualTo(1));
            Assert.That(rig.Population.AliveCount, Is.EqualTo(1));
        }
        
        [Test]
        public void Launch_WithHeading_FliesThatWay()
        {
            var rig = new Rig(new TestAsteroidType("A") { MinSpeed = 2f, MaxSpeed = 2f });

            rig.Population.Launch(Vector2.zero, 90f);
            rig.Loop.Tick(1f);

            var position = rig.Views.Views[0].Entity.Pose.Position;
            Assert.That(position.x, Is.EqualTo(0f).Within(1e-3f));
            Assert.That(position.y, Is.EqualTo(2f).Within(1e-3f));
        }

        [Test]
        public void Launch_WithHeading_DoesNotRollOneOfItsOwn()
        {
            // A rolled heading would take the first value, 0.99, and point almost along +X.
            var rig = new Rig(
                new TestAsteroidType("A") { MinSpeed = 2f, MaxSpeed = 2f }, new ScriptedRandom(0.99f));

            rig.Population.Launch(Vector2.zero, 90f);
            rig.Loop.Tick(1f);

            var position = rig.Views.Views[0].Entity.Pose.Position;
            Assert.That(position.x, Is.EqualTo(0f).Within(1e-3f));
            Assert.That(position.y, Is.EqualTo(2f).Within(1e-3f));
        }

        [Test]
        public void Launch_WithoutHeading_RollsItFirst()
        {
            // The first value, 0.25, makes Range(0, 360) return 90 degrees.
            var rig = new Rig(
                new TestAsteroidType("A") { MinSpeed = 2f, MaxSpeed = 2f }, new ScriptedRandom(0.25f));

            rig.Population.Launch(Vector2.zero);
            rig.Loop.Tick(1f);

            var position = rig.Views.Views[0].Entity.Pose.Position;
            Assert.That(position.x, Is.EqualTo(0f).Within(1e-3f));
            Assert.That(position.y, Is.EqualTo(2f).Within(1e-3f));
        }
    }
}

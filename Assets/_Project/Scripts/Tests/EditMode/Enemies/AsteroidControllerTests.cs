using System;
using System.Collections.Generic;
using Asteroids.Core;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Enemies.Tests
{
    [TestFixture]
    public sealed class AsteroidControllerTests
    {
        private const float Tolerance = 1e-3f;

        private sealed class Rig
        {
            public readonly List<string> Log = new();
            public readonly FakeSimulationLoop Loop = new();
            public readonly FakeDamageRegistry Damage = new();
            public readonly FakeKineticBodyRegistry Bodies = new();
            public readonly RecordingDeathHandler Death;
            public readonly FakeEnemyView View = new();
            public readonly TestAsteroidType Config;
            public readonly AsteroidController Controller;
            public int FinishedCount;

            public Rig(TestAsteroidType config = null)
            {
                Config = config ?? new TestAsteroidType("AsteroidBig");
                Death = new RecordingDeathHandler(Log);
                Controller = new AsteroidController(View, Config, Loop, Damage, Bodies, Death);
                Controller.Finished += _ =>
                {
                    FinishedCount++;
                    Log.Add("finished");
                };
            }

            public IPose Pose => View.Entity.Pose;
        }

        private static AsteroidLaunch Launch(
            float x = 1f, float y = 2f, float angle = 30f, float vx = 1.5f, float vy = 0f, float spin = 90f) =>
            new(new Vector2(x, y), angle, new Vector2(vx, vy), spin);

        private static void AssertPosition(IPose pose, float x, float y)
        {
            Assert.That(pose.Position.x, Is.EqualTo(x).Within(Tolerance));
            Assert.That(pose.Position.y, Is.EqualTo(y).Within(Tolerance));
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            var rig = new Rig();

            Assert.Throws<ArgumentNullException>(() => new AsteroidController(
                null, rig.Config, rig.Loop, rig.Damage, rig.Bodies, rig.Death));
            Assert.Throws<ArgumentNullException>(() => new AsteroidController(
                new FakeEnemyView(), null, rig.Loop, rig.Damage, rig.Bodies, rig.Death));
            Assert.Throws<ArgumentNullException>(() => new AsteroidController(
                new FakeEnemyView(), rig.Config, null, rig.Damage, rig.Bodies, rig.Death));
            Assert.Throws<ArgumentNullException>(() => new AsteroidController(
                new FakeEnemyView(), rig.Config, rig.Loop, null, rig.Bodies, rig.Death));
            Assert.Throws<ArgumentNullException>(() => new AsteroidController(
                new FakeEnemyView(), rig.Config, rig.Loop, rig.Damage, null, rig.Death));
            Assert.Throws<ArgumentNullException>(() => new AsteroidController(
                new FakeEnemyView(), rig.Config, rig.Loop, rig.Damage, rig.Bodies, null));
        }

        [Test]
        public void Constructor_NothingIsInPlayYet()
        {
            var rig = new Rig();

            Assert.That(rig.Controller.IsInPlay, Is.False);
            Assert.That(rig.View.ShowCount, Is.EqualTo(0));
            Assert.That(rig.Loop.LiveCount(SimulationPhase.Motion), Is.EqualTo(0));
            Assert.That(rig.Damage.LiveDamageables, Is.EqualTo(0));
        }

        [Test]
        public void Damage_ReportsContactDamageOfTheType()
        {
            var rig = new Rig(new TestAsteroidType("A") { ContactDamage = 2 });

            Assert.That(rig.Controller.Damage, Is.EqualTo(2));
        }

        [Test]
        public void Launch_ShowsViewAndPlacesPose()
        {
            var rig = new Rig();

            rig.Controller.Launch(Launch(x: 1f, y: 2f, angle: 30f));

            Assert.That(rig.Controller.IsInPlay, Is.True);
            Assert.That(rig.View.IsShown, Is.True);
            AssertPosition(rig.Pose, 1f, 2f);
            Assert.That(rig.Pose.Angle, Is.EqualTo(30f).Within(Tolerance));
        }

        [Test]
        public void Launch_RegistersMotionAndSpinStepsDamageableAndSource()
        {
            var rig = new Rig(new TestAsteroidType("A") { ContactDamage = 2 });

            rig.Controller.Launch(Launch());

            Assert.That(rig.Loop.LiveCount(SimulationPhase.Motion), Is.EqualTo(2));
            Assert.That(rig.Damage.LiveDamageables, Is.EqualTo(1));
            Assert.That(rig.Damage.LiveSources, Is.EqualTo(1));
            Assert.That(rig.Damage.Sources[0].Item.Damage, Is.EqualTo(2));
        }

        [Test]
        public void Launch_RegistersTheAsteroidAsAKineticBodyWithItsVelocity()
        {
            var rig = new Rig(new TestAsteroidType("A"));

            rig.Controller.Launch(Launch(vx: 1.5f, vy: 0f));

            Assert.That(rig.Bodies.LiveCount, Is.EqualTo(1));
            Assert.That(rig.Bodies.Registrations[0].Entity, Is.SameAs(rig.View.Entity));
            Assert.That(rig.Bodies.Registrations[0].Body.MinKnockbackSpeed, Is.EqualTo(0f));
            Assert.That(rig.Bodies.Registrations[0].Body.Velocity.x, Is.EqualTo(1.5f).Within(Tolerance));
        }

        [Test]
        public void Knock_OnTheBody_ChangesTheDirectionOfTravelButNotTheSpin()
        {
            var rig = new Rig(new TestAsteroidType("A") { MaxSpeed = 10f });
            rig.Controller.Launch(Launch(x: 0f, y: 0f, angle: 0f, vx: 1f, vy: 0f, spin: 90f));

            rig.Bodies.Registrations[0].Body.AddVelocity(new Vector2(-2f, 1f));
            rig.Loop.Tick(1f);

            // Velocity becomes (1, 0) + (-2, 1) = (-1, 1).
            AssertPosition(rig.Pose, -1f, 1f);
            Assert.That(rig.Pose.Angle, Is.EqualTo(90f).Within(Tolerance));
        }

        [Test]
        public void Launch_WhileInPlay_Throws()
        {
            var rig = new Rig();
            rig.Controller.Launch(Launch());

            Assert.Throws<InvalidOperationException>(() => rig.Controller.Launch(Launch()));
        }

        [Test]
        public void Tick_MovesAlongVelocityAndSpins()
        {
            var rig = new Rig();
            rig.Controller.Launch(Launch(x: 1f, y: 2f, angle: 30f, vx: 1.5f, vy: 0f, spin: 90f));

            rig.Loop.Tick(0.5f);

            AssertPosition(rig.Pose, 1.75f, 2f);
            Assert.That(rig.Pose.Angle, Is.EqualTo(75f).Within(Tolerance));
        }

        [Test]
        public void Tick_SpinningDoesNotBendTheTravelDirection()
        {
            var rig = new Rig();
            rig.Controller.Launch(Launch(x: 0f, y: 0f, angle: 0f, vx: 1.5f, vy: 0f, spin: 120f));

            for (var i = 0; i < 6; i++) rig.Loop.Tick(0.5f);

            AssertPosition(rig.Pose, 4.5f, 0f);
            Assert.That(rig.Pose.Angle, Is.EqualTo(360f).Within(Tolerance));
        }

        [Test]
        public void Hit_Lethal_TellsTheDeathHandlerWhereItDiedExactlyOnce()
        {
            var rig = new Rig();
            rig.Controller.Launch(Launch(x: 0f, y: 0f, vx: 1.5f, vy: 0f));
            rig.Loop.Tick(1f);

            rig.Damage.HitLatest();

            Assert.That(rig.Death.Deaths, Has.Count.EqualTo(1));
            Assert.That(rig.Death.Deaths[0].x, Is.EqualTo(1.5f).Within(Tolerance));
            Assert.That(rig.Death.Deaths[0].y, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void Hit_Lethal_LeavesPlayAndFinishesOnce()
        {
            var rig = new Rig();
            rig.Controller.Launch(Launch());

            rig.Damage.HitLatest();

            Assert.That(rig.Controller.IsInPlay, Is.False);
            Assert.That(rig.View.IsShown, Is.False);
            Assert.That(rig.Loop.LiveCount(SimulationPhase.Motion), Is.EqualTo(0));
            Assert.That(rig.Damage.LiveDamageables, Is.EqualTo(0));
            Assert.That(rig.Damage.LiveSources, Is.EqualTo(0));
            Assert.That(rig.Bodies.LiveCount, Is.EqualTo(0));
            Assert.That(rig.FinishedCount, Is.EqualTo(1));
        }

        [Test]
        public void Hit_Lethal_NotifiesTheDeathHandlerBeforeFinishing()
        {
            var rig = new Rig();
            rig.Controller.Launch(Launch());

            rig.Damage.HitLatest();

            Assert.That(rig.Log, Is.EqualTo(new[] { "died", "finished" }));
        }

        [Test]
        public void Hit_BelowMaxHealth_StaysInPlay()
        {
            var rig = new Rig(new TestAsteroidType("A") { MaxHealth = 3 });
            rig.Controller.Launch(Launch());

            rig.Damage.HitLatest();

            Assert.That(rig.Controller.IsInPlay, Is.True);
            Assert.That(rig.FinishedCount, Is.EqualTo(0));
            Assert.That(rig.Death.Deaths, Is.Empty);

            rig.Damage.HitLatest(2);

            Assert.That(rig.Controller.IsInPlay, Is.False);
            Assert.That(rig.Death.Deaths, Has.Count.EqualTo(1));
            Assert.That(rig.FinishedCount, Is.EqualTo(1));
        }

        [Test]
        public void Hit_AgainAfterDeath_IsIgnored()
        {
            var rig = new Rig();
            rig.Controller.Launch(Launch());
            var damageable = rig.Damage.Damageables[0].Item;
            damageable.TakeDamage(1);

            damageable.TakeDamage(1);

            Assert.That(rig.Death.Deaths, Has.Count.EqualTo(1));
            Assert.That(rig.FinishedCount, Is.EqualTo(1));
        }

        [Test]
        public void Hit_DeathHandlerThrows_StillLeavesPlayAndRethrows()
        {
            var rig = new Rig();
            rig.Death.ThrowOnDied = new InvalidOperationException("boom");
            rig.Controller.Launch(Launch());

            Assert.Throws<InvalidOperationException>(() => rig.Damage.HitLatest());

            Assert.That(rig.Controller.IsInPlay, Is.False);
            Assert.That(rig.View.IsShown, Is.False);
            Assert.That(rig.FinishedCount, Is.EqualTo(1));
        }

        [Test]
        public void Launch_AfterDeath_RestoresHealthAndRegistrations()
        {
            var rig = new Rig();
            rig.Controller.Launch(Launch());
            rig.Damage.HitLatest();

            rig.Controller.Launch(Launch());

            Assert.That(rig.Controller.IsInPlay, Is.True);
            Assert.That(rig.Damage.LiveDamageables, Is.EqualTo(1));
            Assert.That(rig.Damage.Damageables, Has.Count.EqualTo(2));

            rig.Damage.HitLatest();

            Assert.That(rig.Death.Deaths, Has.Count.EqualTo(2));
            Assert.That(rig.FinishedCount, Is.EqualTo(2));
        }

        [Test]
        public void Launch_AfterDeath_ReplacesTheOldVelocity()
        {
            var rig = new Rig();
            rig.Controller.Launch(Launch(x: 0f, y: 0f, vx: 1.5f, vy: 0f));
            rig.Loop.Tick(1f);
            rig.Damage.HitLatest();

            rig.Controller.Launch(Launch(x: 0f, y: 0f, vx: 0f, vy: 1f));
            rig.Loop.Tick(1f);

            AssertPosition(rig.Pose, 0f, 1f);
        }

        [Test]
        public void Launch_ShowThrows_RollsBackAndRethrows()
        {
            var rig = new Rig();
            rig.View.FailOnShow = true;

            Assert.Throws<InvalidOperationException>(() => rig.Controller.Launch(Launch()));

            Assert.That(rig.Controller.IsInPlay, Is.False);
            Assert.That(rig.View.IsShown, Is.False);
            Assert.That(rig.Loop.LiveCount(SimulationPhase.Motion), Is.EqualTo(0));
            Assert.That(rig.Damage.LiveDamageables, Is.EqualTo(0));
            Assert.That(rig.Damage.LiveSources, Is.EqualTo(0));
            Assert.That(rig.FinishedCount, Is.EqualTo(0));
        }

        [Test]
        public void Dispose_WhileInPlay_LeavesPlayAndDisposesView()
        {
            var rig = new Rig();
            rig.Controller.Launch(Launch());

            rig.Controller.Dispose();

            Assert.That(rig.Controller.IsInPlay, Is.False);
            Assert.That(rig.Loop.LiveCount(SimulationPhase.Motion), Is.EqualTo(0));
            Assert.That(rig.Damage.LiveDamageables, Is.EqualTo(0));
            Assert.That(rig.View.IsDisposed, Is.True);
            Assert.That(rig.FinishedCount, Is.EqualTo(0));
        }
    }
}

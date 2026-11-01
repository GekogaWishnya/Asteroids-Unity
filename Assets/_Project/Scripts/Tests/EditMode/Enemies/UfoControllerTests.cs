using System;
using Asteroids.Core;
using Asteroids.Physics;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Enemies.Tests
{
    [TestFixture]
    public sealed class UfoControllerTests
    {
        private const float Tolerance = 1e-3f;

        private sealed class Rig
        {
            public readonly FakeSimulationLoop Loop = new();
            public readonly FakeDamageRegistry Damage = new();
            public readonly FakeKineticBodyRegistry Bodies = new();
            public readonly RecordingDeathHandler Death = new();
            public readonly FakePlayerTarget Target = FakePlayerTarget.At(0f, 100f);
            public readonly FakeEnemyView View = new(new Entity(new BodyPose()));
            public readonly TestUfoType Config;
            public readonly UfoController Controller;
            public int FinishedCount;

            public Rig(TestUfoType config = null)
            {
                Config = config ?? new TestUfoType();
                Controller = new UfoController(View, Config, Target, Loop, Damage, Bodies, Death);
                Controller.Finished += _ => FinishedCount++;
            }

            public IPose Pose => View.Entity.Pose;
            public IKineticBody Body => Bodies.Registrations[Bodies.Registrations.Count - 1].Body;
            public IDamageable Health => Damage.Damageables[Damage.Damageables.Count - 1].Item;
        }

        private static void AssertVector(Vector2 actual, float x, float y)
        {
            Assert.That(actual.x, Is.EqualTo(x).Within(Tolerance));
            Assert.That(actual.y, Is.EqualTo(y).Within(Tolerance));
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            var rig = new Rig();
            FakeEnemyView View() => new(new Entity(new BodyPose()));

            Assert.Throws<ArgumentNullException>(() => new UfoController(
                null, rig.Config, rig.Target, rig.Loop, rig.Damage, rig.Bodies, rig.Death));
            Assert.Throws<ArgumentNullException>(() => new UfoController(
                View(), null, rig.Target, rig.Loop, rig.Damage, rig.Bodies, rig.Death));
            Assert.Throws<ArgumentNullException>(() => new UfoController(
                View(), rig.Config, null, rig.Loop, rig.Damage, rig.Bodies, rig.Death));
            Assert.Throws<ArgumentNullException>(() => new UfoController(
                View(), rig.Config, rig.Target, null, rig.Damage, rig.Bodies, rig.Death));
            Assert.Throws<ArgumentNullException>(() => new UfoController(
                View(), rig.Config, rig.Target, rig.Loop, null, rig.Bodies, rig.Death));
            Assert.Throws<ArgumentNullException>(() => new UfoController(
                View(), rig.Config, rig.Target, rig.Loop, rig.Damage, null, rig.Death));
            Assert.Throws<ArgumentNullException>(() => new UfoController(
                View(), rig.Config, rig.Target, rig.Loop, rig.Damage, rig.Bodies, null));
        }

        [Test]
        public void Constructor_NothingIsInPlayYet()
        {
            var rig = new Rig();

            Assert.That(rig.Controller.IsInPlay, Is.False);
            Assert.That(rig.View.ShowCount, Is.EqualTo(0));
            Assert.That(rig.Loop.LiveCount(SimulationPhase.Input), Is.EqualTo(0));
            Assert.That(rig.Bodies.LiveCount, Is.EqualTo(0));
        }

        [Test]
        public void Damage_ReportsContactDamageOfTheType()
        {
            var rig = new Rig(new TestUfoType { ContactDamage = 2 });

            Assert.That(rig.Controller.Damage, Is.EqualTo(2));
        }

        [Test]
        public void Launch_ShowsTheViewFacingTheDirectionOfTravel()
        {
            var rig = new Rig();

            rig.Controller.Launch(new Vector2(1f, 2f), new Vector2(-3f, 0f));

            Assert.That(rig.Controller.IsInPlay, Is.True);
            Assert.That(rig.View.IsShown, Is.True);
            AssertVector(rig.Pose.Position, 1f, 2f);
            Assert.That(rig.Pose.Angle, Is.EqualTo(90f).Within(Tolerance));
        }

        [Test]
        public void Launch_RegistersSteeringMotionDamageableSourceAndBody()
        {
            var rig = new Rig();

            rig.Controller.Launch(Vector2.zero, Vector2.up);

            Assert.That(rig.Loop.LiveCount(SimulationPhase.Input), Is.EqualTo(1));
            Assert.That(rig.Loop.LiveCount(SimulationPhase.Motion), Is.EqualTo(1));
            Assert.That(rig.Damage.LiveDamageables, Is.EqualTo(1));
            Assert.That(rig.Damage.LiveSources, Is.EqualTo(1));
            Assert.That(rig.Bodies.LiveCount, Is.EqualTo(1));
            Assert.That(rig.Bodies.Registrations[0].Entity, Is.SameAs(rig.View.Entity));
        }

        [Test]
        public void Launch_FliesAtTheLaunchSpeedWithTheTypesKnockback()
        {
            var rig = new Rig(new TestUfoType { LaunchSpeed = 2f, MinKnockbackSpeed = 3f });

            rig.Controller.Launch(Vector2.zero, new Vector2(0f, -5f));

            AssertVector(rig.Body.Velocity, 0f, -2f);
            Assert.That(rig.Body.MinKnockbackSpeed, Is.EqualTo(3f));
        }

        [TestCase(0f, 0f)]
        [TestCase(float.NaN, 1f)]
        public void Launch_DirectionWithoutLength_Throws(float x, float y)
        {
            var rig = new Rig();

            Assert.Throws<ArgumentException>(() => rig.Controller.Launch(Vector2.zero, new Vector2(x, y)));
            Assert.That(rig.Controller.IsInPlay, Is.False);
        }

        [Test]
        public void Launch_WhileInPlay_Throws()
        {
            var rig = new Rig();
            rig.Controller.Launch(Vector2.zero, Vector2.up);

            Assert.Throws<InvalidOperationException>(() => rig.Controller.Launch(Vector2.zero, Vector2.up));
        }

        [Test]
        public void Launch_ViewFails_LeavesPlayAndRethrows()
        {
            var rig = new Rig();
            rig.View.FailOnShow = true;

            Assert.Throws<InvalidOperationException>(() => rig.Controller.Launch(Vector2.zero, Vector2.up));
            Assert.That(rig.Controller.IsInPlay, Is.False);
            Assert.That(rig.Loop.LiveCount(SimulationPhase.Input), Is.EqualTo(0));
        }

        [Test]
        public void Tick_HomesOnTheShip()
        {
            var rig = new Rig(new TestUfoType { LaunchSpeed = 1f, Thrust = 6f, TurnSpeed = 90f });
            rig.Controller.Launch(Vector2.zero, Vector2.right);

            for (var i = 0; i < 10; i++) rig.Loop.Tick(0.1f);

            // A full second of turning at 90°/s points the nose from +X to +Y, at the far-away ship. The UFO
            // drifted a little to the right meanwhile, so the ship is a fraction of a degree to its left.
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(rig.Pose.Angle, 0f)), Is.LessThan(2f));
            Assert.That(rig.Body.Velocity.y, Is.GreaterThan(0f));
        }

        [Test]
        public void Tick_NoTarget_CoastsAtTheLaunchVelocity()
        {
            var rig = new Rig(new TestUfoType { LaunchSpeed = 2f });
            rig.Target.HasTarget = false;
            rig.Controller.Launch(Vector2.zero, Vector2.right);

            rig.Loop.Tick(1f);

            AssertVector(rig.Pose.Position, 2f, 0f);
            Assert.That(rig.Pose.Angle, Is.EqualTo(270f).Within(Tolerance));
        }

        [Test]
        public void Knockback_StunsTheUfoSoItDriftsAway()
        {
            var rig = new Rig(new TestUfoType { LaunchSpeed = 2f, StunSeconds = 1.5f });
            rig.Controller.Launch(Vector2.zero, Vector2.up);

            rig.Body.AddVelocity(new Vector2(0f, -5f));
            rig.Loop.Tick(1f);

            Assert.That(rig.Controller.IsStunned, Is.True);
            AssertVector(rig.Body.Velocity, 0f, -3f);
            AssertVector(rig.Pose.Position, 0f, -3f);
        }

        [Test]
        public void Knockback_AfterTheStun_HomesAgain()
        {
            var rig = new Rig(new TestUfoType { LaunchSpeed = 2f, Thrust = 6f, StunSeconds = 1f });
            rig.Controller.Launch(Vector2.zero, Vector2.up);
            rig.Body.AddVelocity(new Vector2(0f, -5f));

            rig.Loop.Tick(1f);
            rig.Loop.Tick(0.5f);

            Assert.That(rig.Controller.IsStunned, Is.False);
            AssertVector(rig.Body.Velocity, 0f, 0f);
        }

        [Test]
        public void Hit_NotLethal_DoesNotStun()
        {
            var rig = new Rig(new TestUfoType { MaxHealth = 2 });
            rig.Controller.Launch(Vector2.zero, Vector2.up);

            rig.Health.TakeDamage(1);

            Assert.That(rig.Controller.IsInPlay, Is.True);
            Assert.That(rig.Controller.IsStunned, Is.False);
        }

        [Test]
        public void Hit_Lethal_ReportsWhereItDiedLeavesPlayAndFinishesOnce()
        {
            var rig = new Rig();
            rig.Controller.Launch(new Vector2(3f, 4f), Vector2.up);

            rig.Health.TakeDamage(1);

            Assert.That(rig.Death.Deaths, Has.Count.EqualTo(1));
            AssertVector(rig.Death.Deaths[0], 3f, 4f);
            Assert.That(rig.Controller.IsInPlay, Is.False);
            Assert.That(rig.View.IsShown, Is.False);
            Assert.That(rig.Loop.LiveCount(SimulationPhase.Input), Is.EqualTo(0));
            Assert.That(rig.Loop.LiveCount(SimulationPhase.Motion), Is.EqualTo(0));
            Assert.That(rig.Damage.LiveDamageables, Is.EqualTo(0));
            Assert.That(rig.Damage.LiveSources, Is.EqualTo(0));
            Assert.That(rig.Bodies.LiveCount, Is.EqualTo(0));
            Assert.That(rig.FinishedCount, Is.EqualTo(1));
        }

        [Test]
        public void Hit_Lethal_HandlerThrows_StillLeavesPlay()
        {
            var rig = new Rig();
            rig.Death.ThrowOnDied = new InvalidOperationException("boom");
            rig.Controller.Launch(Vector2.zero, Vector2.up);

            Assert.Throws<InvalidOperationException>(() => rig.Health.TakeDamage(1));
            Assert.That(rig.Controller.IsInPlay, Is.False);
            Assert.That(rig.FinishedCount, Is.EqualTo(1));
        }

        [Test]
        public void Launch_AfterDeath_RefillsHealthAndClearsTheStun()
        {
            var rig = new Rig(new TestUfoType { MaxHealth = 2 });
            rig.Controller.Launch(Vector2.zero, Vector2.up);
            rig.Body.AddVelocity(new Vector2(0f, -1f));
            rig.Health.TakeDamage(2);

            rig.Controller.Launch(Vector2.zero, Vector2.up);
            rig.Health.TakeDamage(1);

            Assert.That(rig.Controller.IsInPlay, Is.True);
            Assert.That(rig.Controller.IsStunned, Is.False);
        }

        [Test]
        public void Dispose_TakesItOutOfPlayAndDestroysTheView()
        {
            var rig = new Rig();
            rig.Controller.Launch(Vector2.zero, Vector2.up);

            rig.Controller.Dispose();

            Assert.That(rig.Controller.IsInPlay, Is.False);
            Assert.That(rig.View.IsDisposed, Is.True);
            Assert.That(rig.FinishedCount, Is.EqualTo(0));
        }
    }
}

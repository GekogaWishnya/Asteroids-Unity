using System;
using Asteroids.Core;
using Asteroids.Weapons.Projectiles;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Weapons.Tests
{
    [TestFixture]
    public sealed class ProjectileTests
    {
        private const float Tolerance = 1e-4f;

        private FakeProjectileView _view;
        private TestWeaponConfig _config;
        private FakeSimulationLoop _loop;
        private FakeDamageRegistry _damage;
        private ProjectileRegistry _projectiles;

        [SetUp]
        public void SetUp()
        {
            _view = new FakeProjectileView();
            _config = new TestWeaponConfig { ProjectileLifetime = 1f, ProjectileSpeed = 10f, ProjectileDamage = 2 };
            _loop = new FakeSimulationLoop();
            _damage = new FakeDamageRegistry();
            _projectiles = new ProjectileRegistry();
        }

        private Projectile Create() => new(_view, _config, _loop, _damage, _projectiles);

        private static ProjectileLaunch Launch(Vector2 position, Vector2 velocity, float angle = 0f) =>
            new(position, angle, velocity);

        [Test]
        public void Construct_RegistersNothing()
        {
            Create();

            Assert.That(_loop.LiveCount(SimulationPhase.Motion), Is.EqualTo(0));
            Assert.That(_loop.LiveCount(SimulationPhase.Logic), Is.EqualTo(0));
            Assert.That(_damage.TryGetSource(_view.Entity, out _), Is.False);
            Assert.That(_projectiles.TryGet(_view.Entity, out _), Is.False);
        }

        [Test]
        public void Launch_ShowsTheViewAtThePositionAndAngle()
        {
            Create().Launch(Launch(new Vector2(1f, 2f), Vector2.up, 45f));

            Assert.That(_view.IsShown, Is.True);
            Assert.That(_view.Entity.Pose.Position, Is.EqualTo(new Vector2(1f, 2f)));
            Assert.That(_view.Entity.Pose.Angle, Is.EqualTo(45f).Within(Tolerance));
        }

        [Test]
        public void Launch_RegistersMotionLifetimeSourceAndProjectile()
        {
            var projectile = Create();

            projectile.Launch(Launch(Vector2.zero, Vector2.up));

            Assert.That(projectile.IsInPlay, Is.True);
            Assert.That(_loop.LiveCount(SimulationPhase.Motion), Is.EqualTo(1));
            Assert.That(_loop.LiveCount(SimulationPhase.Logic), Is.EqualTo(1));
            Assert.That(_damage.TryGetSource(_view.Entity, out var source), Is.True);
            Assert.That(source, Is.SameAs(projectile));
            Assert.That(_projectiles.TryGet(_view.Entity, out var registered), Is.True);
            Assert.That(registered, Is.SameAs(projectile));
        }

        [Test]
        public void Damage_IsTheConfiguredProjectileDamage()
        {
            Assert.That(Create().Damage, Is.EqualTo(2));
        }

        [Test]
        public void Tick_FliesStraightAtTheLaunchVelocity()
        {
            Create().Launch(Launch(Vector2.zero, new Vector2(0f, 10f)));

            _loop.Tick(0.1f);
            _loop.Tick(0.1f);

            Assert.That(_view.Entity.Pose.Position.x, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(_view.Entity.Pose.Position.y, Is.EqualTo(2f).Within(Tolerance));
        }

        [Test]
        public void Tick_BeforeTheLifetimeEnds_StaysInPlay()
        {
            var projectile = Create();
            projectile.Launch(Launch(Vector2.zero, Vector2.up));

            _loop.Tick(0.5f);
            _loop.Tick(0.4f);

            Assert.That(projectile.IsInPlay, Is.True);
        }

        [Test]
        public void Tick_WhenTheLifetimeEnds_LeavesPlayAndRaisesFinishedOnce()
        {
            var projectile = Create();
            var finished = 0;
            projectile.Finished += _ => finished++;
            projectile.Launch(Launch(Vector2.zero, Vector2.up));

            _loop.Tick(0.6f);
            _loop.Tick(0.6f);
            _loop.Tick(0.6f);

            Assert.That(finished, Is.EqualTo(1));
            Assert.That(projectile.IsInPlay, Is.False);
            Assert.That(_view.IsShown, Is.False);
            Assert.That(_loop.LiveCount(SimulationPhase.Motion), Is.EqualTo(0));
            Assert.That(_loop.LiveCount(SimulationPhase.Logic), Is.EqualTo(0));
            Assert.That(_damage.TryGetSource(_view.Entity, out _), Is.False);
            Assert.That(_projectiles.TryGet(_view.Entity, out _), Is.False);
        }

        [Test]
        public void Impact_StaysRegisteredUntilTheLogicPhase()
        {
            var projectile = Create();
            projectile.Launch(Launch(Vector2.zero, Vector2.up));

            projectile.Impact();

            Assert.That(projectile.IsInPlay, Is.True);
            Assert.That(_damage.TryGetSource(_view.Entity, out _), Is.True);
        }

        [Test]
        public void Impact_LeavesPlayOnTheNextTick()
        {
            var projectile = Create();
            var finished = 0;
            projectile.Finished += _ => finished++;
            projectile.Launch(Launch(Vector2.zero, Vector2.up));

            projectile.Impact();
            _loop.Tick(0.01f);

            Assert.That(finished, Is.EqualTo(1));
            Assert.That(projectile.IsInPlay, Is.False);
        }

        [Test]
        public void Impact_WhileNotInPlay_DoesNotCarryOverToTheNextLaunch()
        {
            var projectile = Create();
            projectile.Impact();

            projectile.Launch(Launch(Vector2.zero, Vector2.up));
            _loop.Tick(0.01f);

            Assert.That(projectile.IsInPlay, Is.True);
        }

        [Test]
        public void Launch_AfterFinishing_StartsAFreshLifetime()
        {
            var projectile = Create();
            projectile.Launch(Launch(Vector2.zero, Vector2.up));
            projectile.Impact();
            _loop.Tick(0.01f);

            projectile.Launch(Launch(Vector2.zero, Vector2.up));
            _loop.Tick(0.9f);

            Assert.That(projectile.IsInPlay, Is.True);
        }

        [Test]
        public void Launch_AfterFinishing_UsesTheNewVelocity()
        {
            var projectile = Create();
            projectile.Launch(Launch(Vector2.zero, new Vector2(0f, 10f)));
            _loop.Tick(1.1f);

            projectile.Launch(Launch(Vector2.zero, new Vector2(10f, 0f)));
            _loop.Tick(0.1f);

            Assert.That(_view.Entity.Pose.Position.x, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(_view.Entity.Pose.Position.y, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void Launch_WhileInPlay_Throws()
        {
            var projectile = Create();
            projectile.Launch(Launch(Vector2.zero, Vector2.up));

            Assert.Throws<InvalidOperationException>(() => projectile.Launch(Launch(Vector2.zero, Vector2.up)));
        }

        [Test]
        public void Launch_WhenShowFails_LeavesPlayAndRethrows()
        {
            var projectile = Create();
            _view.FailOnShow = true;

            Assert.Throws<InvalidOperationException>(() => projectile.Launch(Launch(Vector2.zero, Vector2.up)));
            Assert.That(projectile.IsInPlay, Is.False);
            Assert.That(_damage.TryGetSource(_view.Entity, out _), Is.False);
        }

        [Test]
        public void Dispose_LeavesPlayAndDestroysTheView()
        {
            var projectile = Create();
            projectile.Launch(Launch(Vector2.zero, Vector2.up));

            projectile.Dispose();

            Assert.That(projectile.IsInPlay, Is.False);
            Assert.That(_view.IsDisposed, Is.True);
            Assert.That(_projectiles.TryGet(_view.Entity, out _), Is.False);
        }

        [Test]
        public void Construct_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new Projectile(null, _config, _loop, _damage, _projectiles));
            Assert.Throws<ArgumentNullException>(() => new Projectile(_view, null, _loop, _damage, _projectiles));
            Assert.Throws<ArgumentNullException>(() => new Projectile(_view, _config, null, _damage, _projectiles));
            Assert.Throws<ArgumentNullException>(() => new Projectile(_view, _config, _loop, null, _projectiles));
            Assert.Throws<ArgumentNullException>(() => new Projectile(_view, _config, _loop, _damage, null));
        }
    }
}

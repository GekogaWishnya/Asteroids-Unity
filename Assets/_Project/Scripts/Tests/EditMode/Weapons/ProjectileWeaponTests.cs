using System;
using Asteroids.Core;
using Asteroids.Weapons.Projectiles;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Weapons.Tests
{
    [TestFixture]
    public sealed class ProjectileWeaponTests
    {
        private const float Tolerance = 1e-4f;

        private TestWeaponConfig _config;
        private FakeSimulationLoop _loop;
        private FakeProjectileViewFactory _views;
        private ProjectilePool _pool;
        private TestEntity _carrier;
        private TestTrigger _trigger;

        [SetUp]
        public void SetUp()
        {
            _config = new TestWeaponConfig
            {
                FireInterval = 0.25f, MuzzleOffset = 0.5f, ProjectileSpeed = 10f, ProjectileLifetime = 5f
            };
            _loop = new FakeSimulationLoop();
            _views = new FakeProjectileViewFactory();
            _pool = new ProjectilePool(_config, _views, _loop, new FakeDamageRegistry(), new ProjectileRegistry());
            _carrier = new TestEntity();
            _trigger = new TestTrigger();
        }

        private ProjectileWeapon Create() => new(_config, _loop, _pool);

        [Test]
        public void Attach_RegistersOneStepInLogic()
        {
            Create().Attach(_carrier, _trigger);

            Assert.That(_loop.LiveCount(SimulationPhase.Logic), Is.EqualTo(1));
            Assert.That(_loop.LiveCount(SimulationPhase.Input), Is.EqualTo(0));
            Assert.That(_loop.LiveCount(SimulationPhase.Motion), Is.EqualTo(0));
        }

        [Test]
        public void Tick_TriggerNotHeld_DoesNotFire()
        {
            Create().Attach(_carrier, _trigger);

            _loop.Tick(1f);

            Assert.That(_pool.InPlayCount, Is.EqualTo(0));
        }

        [Test]
        public void Tick_TriggerHeld_FiresFromTheMuzzleAlongTheFacing()
        {
            _carrier.Pose.Position = new Vector2(1f, 1f);
            _carrier.Pose.Angle = 90f; // Facing -X.
            Create().Attach(_carrier, _trigger);
            _trigger.IsFiring = true;

            _loop.Tick(0.01f);

            Assert.That(_pool.InPlayCount, Is.EqualTo(1));
            var pose = _views.Views[0].Entity.Pose;
            Assert.That(pose.Position.x, Is.EqualTo(0.5f).Within(Tolerance));
            Assert.That(pose.Position.y, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(pose.Angle, Is.EqualTo(90f).Within(Tolerance));
        }

        [Test]
        public void Tick_AfterFiring_TheProjectileFliesAtTheConfiguredSpeedAlongTheFacing()
        {
            _carrier.Pose.Angle = 90f;
            Create().Attach(_carrier, _trigger);
            _trigger.IsFiring = true;
            _loop.Tick(0.01f);
            _trigger.IsFiring = false;

            _loop.Tick(0.1f);

            var position = _views.Views[0].Entity.Pose.Position;
            Assert.That(position.x, Is.EqualTo(-1.5f).Within(Tolerance));
            Assert.That(position.y, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void Tick_TriggerHeld_FiresOncePerInterval()
        {
            Create().Attach(_carrier, _trigger);
            _trigger.IsFiring = true;

            for (var i = 0; i < 100; i++) _loop.Tick(0.01f);

            Assert.That(_pool.InPlayCount, Is.EqualTo(4));
        }

        [Test]
        public void Tick_FireIntervalChangedAtRuntime_AppliesToTheNextWait()
        {
            Create().Attach(_carrier, _trigger);
            _trigger.IsFiring = true;
            _loop.Tick(0.01f);

            _config.FireInterval = 0.05f;
            for (var i = 0; i < 30; i++) _loop.Tick(0.01f);

            Assert.That(_pool.InPlayCount, Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        public void Dispose_StopsFiring()
        {
            var attachment = Create().Attach(_carrier, _trigger);
            _trigger.IsFiring = true;

            attachment.Dispose();
            _loop.Tick(1f);

            Assert.That(_pool.InPlayCount, Is.EqualTo(0));
            Assert.That(_loop.LiveCount(SimulationPhase.Logic), Is.EqualTo(0));
        }

        [Test]
        public void Dispose_KeepsFiredProjectilesFlying()
        {
            var attachment = Create().Attach(_carrier, _trigger);
            _trigger.IsFiring = true;
            _loop.Tick(0.01f);

            attachment.Dispose();
            _loop.Tick(0.1f);

            Assert.That(_pool.InPlayCount, Is.EqualTo(1));
        }

        [Test]
        public void Attach_Twice_Throws()
        {
            var weapon = Create();
            weapon.Attach(_carrier, _trigger);

            Assert.Throws<InvalidOperationException>(() => weapon.Attach(new TestEntity(), _trigger));
        }

        [Test]
        public void Attach_AfterDispose_Works()
        {
            var weapon = Create();
            weapon.Attach(_carrier, _trigger).Dispose();

            Assert.DoesNotThrow(() => weapon.Attach(_carrier, _trigger));
            Assert.That(_loop.LiveCount(SimulationPhase.Logic), Is.EqualTo(1));
        }

        [Test]
        public void Dispose_Twice_DoesNotDetachALaterAttachment()
        {
            var weapon = Create();
            var first = weapon.Attach(_carrier, _trigger);
            first.Dispose();
            weapon.Attach(_carrier, _trigger);

            first.Dispose();

            Assert.Throws<InvalidOperationException>(() => weapon.Attach(_carrier, _trigger));
        }

        [Test]
        public void Attach_NullArguments_Throw()
        {
            var weapon = Create();

            Assert.Throws<ArgumentNullException>(() => weapon.Attach(null, _trigger));
            Assert.Throws<ArgumentNullException>(() => weapon.Attach(_carrier, null));
        }

        [Test]
        public void Construct_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new ProjectileWeapon(null, _loop, _pool));
            Assert.Throws<ArgumentNullException>(() => new ProjectileWeapon(_config, null, _pool));
            Assert.Throws<ArgumentNullException>(() => new ProjectileWeapon(_config, _loop, null));
        }
    }
}

using System;
using Asteroids.Weapons.Projectiles;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Weapons.Tests
{
    [TestFixture]
    public sealed class ProjectilePoolTests
    {
        private TestWeaponConfig _config;
        private FakeProjectileViewFactory _views;
        private FakeSimulationLoop _loop;

        [SetUp]
        public void SetUp()
        {
            _config = new TestWeaponConfig { ProjectileLifetime = 1f, PoolPrewarm = 3, PoolMaxIdle = 16 };
            _views = new FakeProjectileViewFactory();
            _loop = new FakeSimulationLoop();
        }

        private ProjectilePool Create() =>
            new(_config, _views, _loop, new FakeDamageRegistry(), new ProjectileRegistry());

        private static ProjectileLaunch AnyLaunch => new(Vector2.zero, 0f, Vector2.up);

        [Test]
        public void Prewarm_CreatesTheConfiguredNumberOfHiddenProjectiles()
        {
            Create().Prewarm();

            Assert.That(_views.Views.Count, Is.EqualTo(3));
            Assert.That(_views.ShownCount, Is.EqualTo(0));
        }

        [Test]
        public void Launch_PutsAProjectileInPlay()
        {
            var pool = Create();

            pool.Launch(AnyLaunch);

            Assert.That(pool.InPlayCount, Is.EqualTo(1));
            Assert.That(_views.ShownCount, Is.EqualTo(1));
        }

        [Test]
        public void Launch_WithNothingIdle_GrowsThePool()
        {
            var pool = Create();

            for (var i = 0; i < 5; i++) pool.Launch(AnyLaunch);

            Assert.That(pool.InPlayCount, Is.EqualTo(5));
            Assert.That(_views.Views.Count, Is.EqualTo(5));
        }

        [Test]
        public void Finished_ReturnsTheProjectileToThePoolForReuse()
        {
            var pool = Create();
            pool.Launch(AnyLaunch);
            _loop.Tick(1.1f);

            pool.Launch(AnyLaunch);

            Assert.That(pool.InPlayCount, Is.EqualTo(1));
            Assert.That(_views.Views.Count, Is.EqualTo(1));
            Assert.That(_views.Views[0].ShowCount, Is.EqualTo(2));
        }

        [Test]
        public void Finished_BeyondMaxIdle_DestroysTheProjectile()
        {
            _config.PoolMaxIdle = 0;
            var pool = Create();
            pool.Launch(AnyLaunch);

            _loop.Tick(1.1f);

            Assert.That(pool.InPlayCount, Is.EqualTo(0));
            Assert.That(_views.Views[0].IsDisposed, Is.True);
        }

        [Test]
        public void Launch_WhenTheProjectileFails_ReleasesItAndRethrows()
        {
            var pool = Create();
            pool.Prewarm();
            foreach (var view in _views.Views) view.FailOnShow = true;

            Assert.Throws<InvalidOperationException>(() => pool.Launch(AnyLaunch));
            Assert.That(pool.InPlayCount, Is.EqualTo(0));
        }
    }
}

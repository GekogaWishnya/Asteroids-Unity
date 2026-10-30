using System;
using Asteroids.Weapons.Projectiles;
using NUnit.Framework;

namespace Asteroids.Weapons.Tests
{
    [TestFixture]
    public sealed class ProjectileRegistryTests
    {
        [Test]
        public void Register_ThenTryGet_FindsTheProjectile()
        {
            var registry = new ProjectileRegistry();
            var entity = new TestEntity();
            var projectile = new RecordingImpact();

            registry.Register(entity, projectile);

            Assert.That(registry.TryGet(entity, out var found), Is.True);
            Assert.That(found, Is.SameAs(projectile));
        }

        [Test]
        public void TryGet_UnknownOrNullEntity_ReturnsFalse()
        {
            var registry = new ProjectileRegistry();

            Assert.That(registry.TryGet(new TestEntity(), out _), Is.False);
            Assert.That(registry.TryGet(null, out _), Is.False);
        }

        [Test]
        public void Dispose_RemovesTheRegistration()
        {
            var registry = new ProjectileRegistry();
            var entity = new TestEntity();

            registry.Register(entity, new RecordingImpact()).Dispose();

            Assert.That(registry.TryGet(entity, out _), Is.False);
        }

        [Test]
        public void Dispose_OfAnOldRegistration_DoesNotRemoveANewerOne()
        {
            var registry = new ProjectileRegistry();
            var entity = new TestEntity();
            var old = registry.Register(entity, new RecordingImpact());
            old.Dispose();
            var newer = new RecordingImpact();
            registry.Register(entity, newer);

            old.Dispose();

            Assert.That(registry.TryGet(entity, out var found), Is.True);
            Assert.That(found, Is.SameAs(newer));
        }

        [Test]
        public void Register_SameEntityTwice_Throws()
        {
            var registry = new ProjectileRegistry();
            var entity = new TestEntity();
            registry.Register(entity, new RecordingImpact());

            Assert.Throws<InvalidOperationException>(() => registry.Register(entity, new RecordingImpact()));
        }

        [Test]
        public void Register_NullArguments_Throw()
        {
            var registry = new ProjectileRegistry();

            Assert.Throws<ArgumentNullException>(() => registry.Register(null, new RecordingImpact()));
            Assert.Throws<ArgumentNullException>(() => registry.Register(new TestEntity(), null));
        }
    }
}

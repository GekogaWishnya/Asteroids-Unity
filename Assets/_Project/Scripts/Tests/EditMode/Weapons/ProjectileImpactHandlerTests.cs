using System;
using Asteroids.Collisions;
using Asteroids.Weapons.Projectiles;
using NUnit.Framework;

namespace Asteroids.Weapons.Tests
{
    [TestFixture]
    public sealed class ProjectileImpactHandlerTests
    {
        private ProjectileRegistry _registry;
        private ProjectileImpactHandler _handler;
        private TestEntity _projectileEntity;
        private RecordingImpact _projectile;

        [SetUp]
        public void SetUp()
        {
            _registry = new ProjectileRegistry();
            _handler = new ProjectileImpactHandler(_registry);
            _projectileEntity = new TestEntity();
            _projectile = new RecordingImpact();
            _registry.Register(_projectileEntity, _projectile);
        }

        private static CollisionEntered Contact(object self, object other) =>
            new(new FakeCollider(self), new FakeCollider(other), default);

        [Test]
        public void OnCollisionEntered_ProjectileTouchesAnEntity_Impacts()
        {
            _handler.OnCollisionEntered(Contact(_projectileEntity, new TestEntity()));

            Assert.That(_projectile.Count, Is.EqualTo(1));
        }

        [Test]
        public void OnCollisionEntered_ProjectileTouchesSomethingThatIsNotAnEntity_DoesNothing()
        {
            // The world's walls are owned by the bounds object, which is not an entity.
            _handler.OnCollisionEntered(Contact(_projectileEntity, new object()));

            Assert.That(_projectile.Count, Is.EqualTo(0));
        }

        [Test]
        public void OnCollisionEntered_SideOfTheOtherEntity_DoesNothing()
        {
            _handler.OnCollisionEntered(Contact(new TestEntity(), _projectileEntity));

            Assert.That(_projectile.Count, Is.EqualTo(0));
        }

        [Test]
        public void OnCollisionEntered_AfterTheProjectileLeftPlay_DoesNothing()
        {
            var entity = new TestEntity();
            var projectile = new RecordingImpact();
            _registry.Register(entity, projectile).Dispose();

            _handler.OnCollisionEntered(Contact(entity, new TestEntity()));

            Assert.That(projectile.Count, Is.EqualTo(0));
        }

        [Test]
        public void Construct_NullLookup_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ProjectileImpactHandler(null));
        }
    }
}

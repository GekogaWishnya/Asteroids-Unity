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

        private void Touch(object first, object second) =>
            _handler.Entered(new FakeCollider(first), new FakeCollider(second), default);

        [Test]
        public void Entered_ProjectileTouchesAnEntity_Impacts()
        {
            Touch(_projectileEntity, new TestEntity());

            Assert.That(_projectile.Count, Is.EqualTo(1));
        }

        [Test]
        public void Entered_ProjectileTouchesSomethingThatIsNotAnEntity_DoesNothing()
        {
            // The world's walls are owned by the bounds object, which is not an entity.
            Touch(_projectileEntity, new object());

            Assert.That(_projectile.Count, Is.EqualTo(0));
        }

        [Test]
        public void Entered_ProjectileIsTheSecondCollider_Impacts()
        {
            Touch(new TestEntity(), _projectileEntity);

            Assert.That(_projectile.Count, Is.EqualTo(1));
        }

        [Test]
        public void Entered_TwoProjectilesMeet_BothImpact()
        {
            var otherEntity = new TestEntity();
            var other = new RecordingImpact();
            _registry.Register(otherEntity, other);

            Touch(_projectileEntity, otherEntity);

            Assert.That(_projectile.Count, Is.EqualTo(1));
            Assert.That(other.Count, Is.EqualTo(1));
        }

        [Test]
        public void Entered_NeitherSideIsAProjectile_DoesNothing()
        {
            Assert.DoesNotThrow(() => Touch(new TestEntity(), new TestEntity()));
            Assert.That(_projectile.Count, Is.EqualTo(0));
        }

        [Test]
        public void StayedAndExited_DoNothing()
        {
            var first = new FakeCollider(_projectileEntity);
            var second = new FakeCollider(new TestEntity());

            _handler.Stayed(first, second, default);
            _handler.Exited(first, second);

            Assert.That(_projectile.Count, Is.EqualTo(0));
        }

        [Test]
        public void Order_IsProjectileImpact()
        {
            Assert.That(_handler.Order, Is.EqualTo(CollisionResponseOrder.ProjectileImpact));
        }

        [Test]
        public void Entered_AfterTheProjectileLeftPlay_DoesNothing()
        {
            var entity = new TestEntity();
            var projectile = new RecordingImpact();
            _registry.Register(entity, projectile).Dispose();

            Touch(entity, new TestEntity());

            Assert.That(projectile.Count, Is.EqualTo(0));
        }

        [Test]
        public void Construct_NullLookup_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ProjectileImpactHandler(null));
        }
    }
}

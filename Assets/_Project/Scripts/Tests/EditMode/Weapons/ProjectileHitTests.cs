using Asteroids.Collisions;
using Asteroids.Damage;
using Asteroids.Weapons.Projectiles;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Weapons.Tests
{
    /// <summary>
    /// A real projectile against the real damage and impact responses, in the order the collision system
    /// calls them: one bullet stops at the first entity it touches, even when it touches several in one pass.
    /// </summary>
    [TestFixture]
    public sealed class ProjectileHitTests
    {
        private DamageRegistry _damage;
        private ProjectileRegistry _projectiles;
        private FakeProjectileView _view;
        private FakeSimulationLoop _loop;
        private Projectile _projectile;

        [SetUp]
        public void SetUp()
        {
            _damage = new DamageRegistry();
            _projectiles = new ProjectileRegistry();
            _view = new FakeProjectileView();
            _loop = new FakeSimulationLoop();
            var config = new TestWeaponConfig { ProjectileLifetime = 1f, ProjectileSpeed = 10f, ProjectileDamage = 1 };
            _projectile = new Projectile(_view, config, _loop, new FakeWrapRegistry(), _damage, _projectiles);
            _projectile.Launch(new ProjectileLaunch(Vector2.zero, 0f, Vector2.up));
        }

        private ICollisionEventSink CreateSink(bool bindImpactFirst)
        {
            var damage = new ContactDamageHandler(_damage);
            var impact = new ProjectileImpactHandler(_projectiles);
            var bound = bindImpactFirst
                ? new ICollisionResponse[] { impact, damage }
                : new ICollisionResponse[] { damage, impact };
            return new CompositeCollisionEventSink(CollisionResponses.InOrder(bound));
        }

        private RecordingDamageable AddEnemy(out FakeCollider collider)
        {
            var entity = new TestEntity();
            var health = new RecordingDamageable();
            _damage.RegisterDamageable(entity, health);
            collider = new FakeCollider(entity);
            return health;
        }

        [TestCase(false)]
        [TestCase(true)]
        public void OneBulletTouchingTwoEnemiesInOnePass_HurtsOnlyTheFirst(bool bindImpactFirst)
        {
            var sink = CreateSink(bindImpactFirst);
            var bullet = new FakeCollider(_view.Entity);
            var firstHealth = AddEnemy(out var first);
            var secondHealth = AddEnemy(out var second);

            sink.Entered(bullet, first, default);
            sink.Entered(second, bullet, default);

            Assert.That(firstHealth.Hits, Is.EqualTo(new[] { 1 }));
            Assert.That(secondHealth.Hits, Is.Empty);
        }

        [Test]
        public void BulletTouchingSomethingHarmlessFirst_StopsThereAndHurtsNothingAfter()
        {
            var sink = CreateSink(bindImpactFirst: false);
            var bullet = new FakeCollider(_view.Entity);
            var harmless = new FakeCollider(new TestEntity());
            var health = AddEnemy(out var enemy);

            sink.Entered(bullet, harmless, default);
            sink.Entered(bullet, enemy, default);

            Assert.That(health.Hits, Is.Empty);
        }

        [Test]
        public void SpentBullet_LeavesPlayInTheLogicPhase()
        {
            var sink = CreateSink(bindImpactFirst: false);
            AddEnemy(out var enemy);

            sink.Entered(new FakeCollider(_view.Entity), enemy, default);
            _loop.Tick(0.01f);

            Assert.That(_projectile.IsInPlay, Is.False);
        }
    }
}

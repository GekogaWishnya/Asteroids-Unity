using System;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Physics;
using NUnit.Framework;

namespace Asteroids.Player.Tests
{
    [TestFixture]
    public sealed class ShipDefenseTests
    {
        private FakeSimulationLoop _loop;
        private TestCombatConfig _combat;
        private PlayerHealth _health;
        private FakeDamageRegistry _registry;
        private FakeCollisionIgnore _collider;
        private FakeProtectionView _view;
        private FakeProtectionTargets _targets;
        private WaveStartProtection _waveProtection;
        private Entity _ship;

        [SetUp]
        public void SetUp()
        {
            _loop = new FakeSimulationLoop();
            _combat = new TestCombatConfig { InvulnerabilitySeconds = 1.5f };
            _health = new PlayerHealth(new TestHealthConfig { MaxHealth = 3 });
            _registry = new FakeDamageRegistry();
            _collider = new FakeCollisionIgnore();
            _view = new FakeProtectionView();
            _targets = new FakeProtectionTargets(new[] { _collider }, new[] { _view });
            _waveProtection = new WaveStartProtection(_combat);
            _ship = new Entity(new BodyPose());
        }

        private ShipDefense Create() => new(_health, _combat, _targets, _waveProtection, _loop, _registry);

        /// <summary>What the damage system sees when it hurts the ship.</summary>
        private IDamageable RegisteredShip => _registry.Damageables[_registry.Damageables.Count - 1].Item;

        [Test]
        public void Construct_RegistersNothing()
        {
            Create();

            Assert.That(_registry.Damageables, Is.Empty);
            Assert.That(_loop.Entries, Is.Empty);
        }

        [Test]
        public void Attach_RegistersTheShipAsDamageableAndTheCountdownInLogic()
        {
            Create().Attach(_ship);

            Assert.That(_registry.LiveDamageables, Is.EqualTo(1));
            Assert.That(_registry.Damageables[0].Entity, Is.SameAs(_ship));
            Assert.That(_loop.LiveCount(SimulationPhase.Logic), Is.EqualTo(1));
            Assert.That(_loop.Entries.Count, Is.EqualTo(1));
        }

        [Test]
        public void Attach_DamageThroughTheRegistration_HurtsThePlayer()
        {
            Create().Attach(_ship);

            RegisteredShip.TakeDamage(1);

            Assert.That(_health.Current.Value, Is.EqualTo(2));
        }

        [Test]
        public void Dispose_RemovesTheDamageableAndTheCountdown()
        {
            Create().Attach(_ship).Dispose();

            Assert.That(_registry.LiveDamageables, Is.EqualTo(0));
            Assert.That(_loop.LiveCount(SimulationPhase.Logic), Is.EqualTo(0));
        }

        [Test]
        public void Attach_Twice_Throws()
        {
            var defense = Create();
            defense.Attach(_ship);

            Assert.Throws<InvalidOperationException>(() => defense.Attach(_ship));
        }

        [Test]
        public void Hit_ProtectsTheShipFromTheNextHit()
        {
            Create().Attach(_ship);

            RegisteredShip.TakeDamage(1);
            RegisteredShip.TakeDamage(1);

            Assert.That(_health.Current.Value, Is.EqualTo(2));
        }

        [Test]
        public void Hit_AfterTheProtectionRanOut_HurtsAgain()
        {
            Create().Attach(_ship);
            RegisteredShip.TakeDamage(1);

            _loop.Tick(1.4f);
            RegisteredShip.TakeDamage(1);
            Assert.That(_health.Current.Value, Is.EqualTo(2));

            _loop.Tick(0.2f);
            RegisteredShip.TakeDamage(1);
            Assert.That(_health.Current.Value, Is.EqualTo(1));
        }

        [Test]
        public void Hit_WithoutInvulnerability_EveryHitCounts()
        {
            _combat.InvulnerabilitySeconds = 0f;
            Create().Attach(_ship);

            RegisteredShip.TakeDamage(1);
            RegisteredShip.TakeDamage(1);

            Assert.That(_health.Current.Value, Is.EqualTo(1));
        }

        [Test]
        public void Attach_AfterDispose_StartsWithoutProtection()
        {
            var defense = Create();
            var attachment = defense.Attach(_ship);
            RegisteredShip.TakeDamage(1);
            attachment.Dispose();
            defense.Attach(_ship);

            RegisteredShip.TakeDamage(1);

            Assert.That(_health.Current.Value, Is.EqualTo(1));
        }

        [Test]
        public void Hit_MakesTheCollidersIgnoreTheConfiguredLayersUntilTheWindowEnds()
        {
            _combat.IgnoredLayers = CollisionLayer.Enemy | CollisionLayer.PlayerProjectile;
            Create().Attach(_ship);

            RegisteredShip.TakeDamage(1);
            Assert.That(_collider.Ignored, Is.EqualTo(CollisionLayer.Enemy | CollisionLayer.PlayerProjectile));

            _loop.Tick(1.4f);
            Assert.That(_collider.Ignored, Is.EqualTo(CollisionLayer.Enemy | CollisionLayer.PlayerProjectile));

            _loop.Tick(0.2f);
            Assert.That(_collider.Ignored, Is.EqualTo(CollisionLayer.None));
        }

        [Test]
        public void Dispose_DuringTheWindow_RestoresTheColliders()
        {
            var attachment = Create().Attach(_ship);
            RegisteredShip.TakeDamage(1);

            attachment.Dispose();

            Assert.That(_collider.Ignored, Is.EqualTo(CollisionLayer.None));
        }

        [Test]
        public void Hit_WithoutInvulnerability_LeavesTheCollidersAlone()
        {
            _combat.InvulnerabilitySeconds = 0f;
            Create().Attach(_ship);

            RegisteredShip.TakeDamage(1);

            Assert.That(_collider.Ignored, Is.EqualTo(CollisionLayer.None));
        }

        [Test]
        public void Hit_TellsTheViewsForTheWholeWindow()
        {
            Create().Attach(_ship);

            RegisteredShip.TakeDamage(1);
            Assert.That(_view.Calls, Is.EqualTo(new[] { true }));

            _loop.Tick(1.4f);
            Assert.That(_view.Calls, Is.EqualTo(new[] { true }));

            _loop.Tick(0.2f);
            Assert.That(_view.Calls, Is.EqualTo(new[] { true, false }));
        }

        [Test]
        public void Dispose_DuringProtection_TellsTheViewsItEnded()
        {
            var attachment = Create().Attach(_ship);
            RegisteredShip.TakeDamage(1);

            attachment.Dispose();

            Assert.That(_view.Calls, Is.EqualTo(new[] { true, false }));
        }

        [Test]
        public void Hit_WithoutInvulnerability_NeverTellsTheViews()
        {
            _combat.InvulnerabilitySeconds = 0f;
            Create().Attach(_ship);

            RegisteredShip.TakeDamage(1);

            Assert.That(_view.Calls, Is.Empty);
        }

        [Test]
        public void WaveStart_ProtectsTheShipLikeAHitWould()
        {
            Create().Attach(_ship);

            _waveProtection.OnWaveStarted(new WaveStarted(1, 3));
            RegisteredShip.TakeDamage(1);

            Assert.That(_health.Current.Value, Is.EqualTo(3));
            Assert.That(_collider.Ignored, Is.EqualTo(CollisionLayer.Enemy));
            Assert.That(_view.Calls, Is.EqualTo(new[] { true }));

            _loop.Tick(2f);
            RegisteredShip.TakeDamage(1);

            Assert.That(_health.Current.Value, Is.EqualTo(2));
        }

        [Test]
        public void WaveStart_BeforeAttaching_ProtectsNothingLater()
        {
            var defense = Create();

            _waveProtection.OnWaveStarted(new WaveStarted(1, 3));
            defense.Attach(_ship);
            RegisteredShip.TakeDamage(1);

            Assert.That(_health.Current.Value, Is.EqualTo(2));
        }

        [Test]
        public void WaveStart_AfterDispose_DoesNotThrowOrProtect()
        {
            Create().Attach(_ship).Dispose();

            Assert.DoesNotThrow(() => _waveProtection.OnWaveStarted(new WaveStarted(1, 3)));
            Assert.That(_collider.Ignored, Is.EqualTo(CollisionLayer.None));
            Assert.That(_view.Calls, Is.Empty);
        }

        [Test]
        public void WaveStart_ZeroSeconds_ProtectsNothing()
        {
            _combat.WaveStartProtectionSeconds = 0f;
            Create().Attach(_ship);

            _waveProtection.OnWaveStarted(new WaveStarted(1, 3));
            RegisteredShip.TakeDamage(1);

            Assert.That(_health.Current.Value, Is.EqualTo(2));
        }

        [Test]
        public void WaveStart_DuringALongerHitWindow_DoesNotShortenIt()
        {
            _combat.InvulnerabilitySeconds = 5f;
            _combat.WaveStartProtectionSeconds = 1f;
            Create().Attach(_ship);
            RegisteredShip.TakeDamage(1);

            _waveProtection.OnWaveStarted(new WaveStarted(2, 3));
            _loop.Tick(4.9f);
            RegisteredShip.TakeDamage(1);

            Assert.That(_health.Current.Value, Is.EqualTo(2));
        }

        [Test]
        public void Attach_NullShip_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Create().Attach(null));
        }

        [Test]
        public void Construct_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ShipDefense(null, _combat, _targets, _waveProtection, _loop, _registry));
            Assert.Throws<ArgumentNullException>(() =>
                new ShipDefense(_health, null, _targets, _waveProtection, _loop, _registry));
            Assert.Throws<ArgumentNullException>(() =>
                new ShipDefense(_health, _combat, null, _waveProtection, _loop, _registry));
            Assert.Throws<ArgumentNullException>(() =>
                new ShipDefense(_health, _combat, _targets, null, _loop, _registry));
            Assert.Throws<ArgumentNullException>(() =>
                new ShipDefense(_health, _combat, _targets, _waveProtection, null, _registry));
            Assert.Throws<ArgumentNullException>(() =>
                new ShipDefense(_health, _combat, _targets, _waveProtection, _loop, null));
        }
    }
}

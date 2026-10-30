using System;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Physics;
using NUnit.Framework;

namespace Asteroids.Player.Tests
{
    [TestFixture]
    public sealed class ShipControllerTests
    {
        private const float Dt = 0.1f;
        private const float Tolerance = 1e-4f;

        private FakeSimulationLoop _loop;
        private TestInput _input;
        private TestConfig _config;
        private TestCombatConfig _combat;
        private BodyPose _pose;
        private Entity _entity;
        private PlayerHealth _health;
        private FakeDamageRegistry _registry;
        private RecordingShipEvents _events;
        private FakeCollisionIgnore _collider;
        private FakeProtectionView _view;
        private FakeProtectionTargets _targets;
        private WaveStartProtection _waveProtection;
        private FakeWeapon _weapon;

        [SetUp]
        public void SetUp()
        {
            _loop = new FakeSimulationLoop();
            _input = new TestInput();
            _config = new TestConfig { Thrust = 10f, TurnSpeed = 100f, MaxSpeed = 100f, LinearDamping = 0f };
            _combat = new TestCombatConfig { RamDamage = 1, InvulnerabilitySeconds = 1.5f };
            _pose = new BodyPose();
            _entity = new Entity(_pose);
            _health = new PlayerHealth(new TestHealthConfig { MaxHealth = 3 });
            _registry = new FakeDamageRegistry();
            _events = new RecordingShipEvents();
            _collider = new FakeCollisionIgnore();
            _view = new FakeProtectionView();
            _targets = new FakeProtectionTargets(new[] { _collider }, new[] { _view });
            _waveProtection = new WaveStartProtection(_combat);
            _weapon = new FakeWeapon();
        }

        private ShipController CreateController() =>
            new(_input, _input, _config, _combat, _combat, _loop, _entity, _targets, _waveProtection,
                _health, _registry, _weapon, _events);

        /// <summary>What the damage system sees when it hurts the ship.</summary>
        private IDamageable RegisteredShip => _registry.Damageables[0].Item;

        [Test]
        public void Enable_RegistersOneStepInInputOneInMotionAndOneInLogic()
        {
            CreateController().Enable();

            Assert.That(_loop.Entries.Count, Is.EqualTo(3));
            Assert.That(_loop.LiveCount(SimulationPhase.Input), Is.EqualTo(1));
            Assert.That(_loop.LiveCount(SimulationPhase.Motion), Is.EqualTo(1));
            Assert.That(_loop.LiveCount(SimulationPhase.Logic), Is.EqualTo(1));
            Assert.That(_loop.LiveCount(SimulationPhase.Collision), Is.EqualTo(0));
        }

        [Test]
        public void Construct_DoesNotRegisterUntilEnabled()
        {
            CreateController();

            Assert.That(_loop.Entries, Is.Empty);
        }

        [Test]
        public void Disable_DisposesAllStepRegistrations()
        {
            var controller = CreateController();
            controller.Enable();

            controller.Disable();

            Assert.That(_loop.Entries.Count, Is.EqualTo(3));
            Assert.That(_loop.Entries, Has.All.Matches<FakeSimulationLoop.Entry>(e => e.IsDisposed));
        }

        [Test]
        public void Enable_Twice_RegistersOnce()
        {
            var controller = CreateController();

            controller.Enable();
            controller.Enable();

            Assert.That(_loop.Entries.Count, Is.EqualTo(3));
        }

        [Test]
        public void Disable_WithoutEnable_DoesNothing()
        {
            CreateController().Disable();

            Assert.That(_loop.Entries, Is.Empty);
        }

        [Test]
        public void EnableDisableEnable_RegistersAgain()
        {
            var controller = CreateController();

            controller.Enable();
            controller.Disable();
            controller.Enable();

            Assert.That(_loop.Entries.Count, Is.EqualTo(6));
            Assert.That(_loop.LiveCount(SimulationPhase.Input), Is.EqualTo(1));
            Assert.That(_loop.LiveCount(SimulationPhase.Motion), Is.EqualTo(1));
            Assert.That(_loop.LiveCount(SimulationPhase.Logic), Is.EqualTo(1));
        }

        [Test]
        public void Tick_WithTurnInput_DecreasesAngleByTurnSpeedTimesDelta()
        {
            _pose.Angle = 90f;
            _input.Turn = 1f;
            CreateController().Enable();

            _loop.Tick(Dt);

            Assert.That(_pose.Angle, Is.EqualTo(90f - _config.TurnSpeed * Dt).Within(Tolerance));
        }

        [Test]
        public void Tick_WithThrustInput_MovesAlongUp()
        {
            _input.Thrust = 1f;
            CreateController().Enable();

            _loop.Tick(Dt);

            // v = Thrust * dt, displacement = v * dt, facing +Y at angle 0.
            Assert.That(_pose.Position.x, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(_pose.Position.y, Is.EqualTo(_config.Thrust * Dt * Dt).Within(Tolerance));
        }

        [Test]
        public void Tick_ThrustAboveMaxSpeed_ClampsDisplacement()
        {
            _config.MaxSpeed = 0.5f;
            _input.Thrust = 1f;
            CreateController().Enable();

            _loop.Tick(Dt);

            Assert.That(_pose.Position.y, Is.EqualTo(_config.MaxSpeed * Dt).Within(Tolerance));
        }

        [Test]
        public void Tick_AfterDisable_DoesNotMoveOrTurn()
        {
            _input.Turn = 1f;
            _input.Thrust = 1f;
            var controller = CreateController();
            controller.Enable();
            controller.Disable();

            _loop.Tick(Dt);

            Assert.That(_pose.Angle, Is.EqualTo(0f));
            Assert.That(_pose.Position, Is.EqualTo(UnityEngine.Vector2.zero));
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new ShipController(
                null, _input, _config, _combat, _combat, _loop, _entity,
                _targets, _waveProtection, _health, _registry, _weapon, _events));
            Assert.Throws<ArgumentNullException>(() => new ShipController(
                _input, null, _config, _combat, _combat, _loop, _entity,
                _targets, _waveProtection, _health, _registry, _weapon, _events));
            Assert.Throws<ArgumentNullException>(() => new ShipController(
                _input, _input, null, _combat, _combat, _loop, _entity,
                _targets, _waveProtection, _health, _registry, _weapon, _events));
            Assert.Throws<ArgumentNullException>(() => new ShipController(
                _input, _input, _config, null, _combat, _loop, _entity,
                _targets, _waveProtection, _health, _registry, _weapon, _events));
            Assert.Throws<ArgumentNullException>(() => new ShipController(
                _input, _input, _config, _combat, null, _loop, _entity,
                _targets, _waveProtection, _health, _registry, _weapon, _events));
            Assert.Throws<ArgumentNullException>(() => new ShipController(
                _input, _input, _config, _combat, _combat, null, _entity,
                _targets, _waveProtection, _health, _registry, _weapon, _events));
            Assert.Throws<ArgumentNullException>(() => new ShipController(
                _input, _input, _config, _combat, _combat, _loop, null,
                _targets, _waveProtection, _health, _registry, _weapon, _events));
            Assert.Throws<ArgumentNullException>(() => new ShipController(
                _input, _input, _config, _combat, _combat, _loop, _entity,
                null, _waveProtection, _health, _registry, _weapon, _events));
            Assert.Throws<ArgumentNullException>(() => new ShipController(
                _input, _input, _config, _combat, _combat, _loop, _entity,
                _targets, null, _health, _registry, _weapon, _events));
            Assert.Throws<ArgumentNullException>(() => new ShipController(
                _input, _input, _config, _combat, _combat, _loop, _entity,
                _targets, _waveProtection, null, _registry, _weapon, _events));
            Assert.Throws<ArgumentNullException>(() => new ShipController(
                _input, _input, _config, _combat, _combat, _loop, _entity,
                _targets, _waveProtection, _health, null, _weapon, _events));
            Assert.Throws<ArgumentNullException>(() => new ShipController(
                _input, _input, _config, _combat, _combat, _loop, _entity,
                _targets, _waveProtection, _health, _registry, null, _events));
            Assert.Throws<ArgumentNullException>(() => new ShipController(
                _input, _input, _config, _combat, _combat, _loop, _entity,
                _targets, _waveProtection, _health, _registry, _weapon, null));
        }

        [Test]
        public void Enable_RegistersTheShipAsDamageableOfItsEntity()
        {
            CreateController().Enable();

            Assert.That(_registry.LiveDamageables, Is.EqualTo(1));
            Assert.That(_registry.Damageables[0].Entity, Is.SameAs(_entity));

            RegisteredShip.TakeDamage(1);

            Assert.That(_health.Current.Value, Is.EqualTo(2));
        }

        [Test]
        public void Construct_DoesNotRegisterAsDamageableUntilEnabled()
        {
            CreateController();

            Assert.That(_registry.Damageables, Is.Empty);
        }

        [Test]
        public void Enable_Twice_RegistersAsDamageableOnce()
        {
            var controller = CreateController();

            controller.Enable();
            controller.Enable();

            Assert.That(_registry.Damageables.Count, Is.EqualTo(1));
        }

        [Test]
        public void Disable_RemovesTheDamageableRegistration()
        {
            var controller = CreateController();
            controller.Enable();

            controller.Disable();

            Assert.That(_registry.LiveDamageables, Is.EqualTo(0));
        }

        [Test]
        public void Enable_RegistersTheShipAsDamageSourceOfItsEntity()
        {
            _combat.RamDamage = 2;

            CreateController().Enable();

            Assert.That(_registry.LiveSources, Is.EqualTo(1));
            Assert.That(_registry.Sources[0].Entity, Is.SameAs(_entity));
            Assert.That(_registry.Sources[0].Item.Damage, Is.EqualTo(2));
        }

        [Test]
        public void Construct_DoesNotRegisterAsDamageSourceUntilEnabled()
        {
            CreateController();

            Assert.That(_registry.Sources, Is.Empty);
        }

        [Test]
        public void Disable_RemovesTheDamageSourceRegistration()
        {
            var controller = CreateController();
            controller.Enable();

            controller.Disable();

            Assert.That(_registry.LiveSources, Is.EqualTo(0));
        }

        [Test]
        public void Damage_NotLethal_DoesNotDestroyTheShip()
        {
            CreateController().Enable();

            _health.TakeDamage(2);

            Assert.That(_events.DestroyedEntities, Is.Empty);
            Assert.That(_loop.LiveCount(SimulationPhase.Input), Is.EqualTo(1));
            Assert.That(_registry.LiveDamageables, Is.EqualTo(1));
        }

        [Test]
        public void Damage_Lethal_ReportsDestroyedOnceWithTheEntity()
        {
            CreateController().Enable();

            _health.TakeDamage(3);
            _health.TakeDamage(1);

            Assert.That(_events.DestroyedEntities, Is.EqualTo(new[] { _entity }));
        }

        [Test]
        public void Damage_Lethal_DisablesTheShipBeforeReportingIt()
        {
            var liveStepsWhenReported = -1;
            var liveDamageablesWhenReported = -1;
            var liveSourcesWhenReported = -1;
            _events.OnDestroyed = () =>
            {
                liveStepsWhenReported = _loop.LiveCount(SimulationPhase.Input) +
                                        _loop.LiveCount(SimulationPhase.Motion) +
                                        _loop.LiveCount(SimulationPhase.Logic);
                liveDamageablesWhenReported = _registry.LiveDamageables;
                liveSourcesWhenReported = _registry.LiveSources;
            };
            CreateController().Enable();

            _health.TakeDamage(3);

            Assert.That(liveStepsWhenReported, Is.EqualTo(0));
            Assert.That(liveDamageablesWhenReported, Is.EqualTo(0));
            Assert.That(liveSourcesWhenReported, Is.EqualTo(0));
        }

        [Test]
        public void Tick_AfterDestruction_DoesNotMoveOrTurn()
        {
            _input.Turn = 1f;
            _input.Thrust = 1f;
            CreateController().Enable();
            _health.TakeDamage(3);

            _loop.Tick(Dt);

            Assert.That(_pose.Angle, Is.EqualTo(0f));
            Assert.That(_pose.Position, Is.EqualTo(UnityEngine.Vector2.zero));
        }

        [Test]
        public void Enable_WhenHealthIsDepleted_DoesNothing()
        {
            _health.TakeDamage(3);

            CreateController().Enable();

            Assert.That(_loop.Entries, Is.Empty);
            Assert.That(_registry.Damageables, Is.Empty);
            Assert.That(_registry.Sources, Is.Empty);
        }

        [Test]
        public void Enable_AfterHealthWasReset_WorksAgain()
        {
            var controller = CreateController();
            controller.Enable();
            _health.TakeDamage(3);
            _health.Reset();

            controller.Enable();

            Assert.That(_loop.LiveCount(SimulationPhase.Input), Is.EqualTo(1));
            Assert.That(_registry.LiveDamageables, Is.EqualTo(1));
            Assert.That(_registry.LiveSources, Is.EqualTo(1));
        }

        [Test]
        public void Damage_AfterDisable_DoesNotReportDestroyed()
        {
            var controller = CreateController();
            controller.Enable();
            controller.Disable();

            _health.TakeDamage(3);

            Assert.That(_events.DestroyedEntities, Is.Empty);
        }

        [Test]
        public void Damage_AfterReEnable_ReportsDestroyedOnce()
        {
            var controller = CreateController();
            controller.Enable();
            controller.Disable();
            controller.Enable();

            _health.TakeDamage(3);

            Assert.That(_events.DestroyedEntities.Count, Is.EqualTo(1));
        }

        [Test]
        public void Hit_ThroughTheDamageSystem_ProtectsTheShipFromTheNextHit()
        {
            CreateController().Enable();

            RegisteredShip.TakeDamage(1);
            RegisteredShip.TakeDamage(1);

            Assert.That(_health.Current.Value, Is.EqualTo(2));
        }

        [Test]
        public void Hit_AfterTheProtectionRanOut_HurtsAgain()
        {
            CreateController().Enable();
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
            CreateController().Enable();

            RegisteredShip.TakeDamage(1);
            RegisteredShip.TakeDamage(1);

            Assert.That(_health.Current.Value, Is.EqualTo(1));
        }

        [Test]
        public void Enable_AfterDisable_StartsWithoutProtection()
        {
            var controller = CreateController();
            controller.Enable();
            RegisteredShip.TakeDamage(1);
            controller.Disable();
            controller.Enable();

            _registry.Damageables[1].Item.TakeDamage(1);

            Assert.That(_health.Current.Value, Is.EqualTo(1));
        }

        [Test]
        public void Hit_MakesTheCollidersIgnoreTheConfiguredLayersUntilTheWindowEnds()
        {
            _combat.IgnoredLayers = CollisionLayer.Enemy | CollisionLayer.PlayerProjectile;
            CreateController().Enable();

            RegisteredShip.TakeDamage(1);
            Assert.That(_collider.Ignored, Is.EqualTo(CollisionLayer.Enemy | CollisionLayer.PlayerProjectile));

            _loop.Tick(1.4f);
            Assert.That(_collider.Ignored, Is.EqualTo(CollisionLayer.Enemy | CollisionLayer.PlayerProjectile));

            _loop.Tick(0.2f);
            Assert.That(_collider.Ignored, Is.EqualTo(CollisionLayer.None));
        }

        [Test]
        public void Disable_DuringTheWindow_RestoresTheColliders()
        {
            var controller = CreateController();
            controller.Enable();
            RegisteredShip.TakeDamage(1);

            controller.Disable();

            Assert.That(_collider.Ignored, Is.EqualTo(CollisionLayer.None));
        }

        [Test]
        public void Hit_Lethal_RestoresTheCollidersBeforeTheShipIsReportedDestroyed()
        {
            CollisionLayer? ignoredWhenReported = null;
            _events.OnDestroyed = () => ignoredWhenReported = _collider.Ignored;
            CreateController().Enable();

            RegisteredShip.TakeDamage(3);

            Assert.That(ignoredWhenReported, Is.EqualTo(CollisionLayer.None));
        }

        [Test]
        public void Hit_WithoutInvulnerability_LeavesTheCollidersAlone()
        {
            _combat.InvulnerabilitySeconds = 0f;
            CreateController().Enable();

            RegisteredShip.TakeDamage(1);

            Assert.That(_collider.Ignored, Is.EqualTo(CollisionLayer.None));
        }

        [Test]
        public void Hit_TellsTheViewsForTheWholeWindow()
        {
            CreateController().Enable();

            RegisteredShip.TakeDamage(1);
            Assert.That(_view.Calls, Is.EqualTo(new[] { true }));

            _loop.Tick(1.4f);
            Assert.That(_view.Calls, Is.EqualTo(new[] { true }));

            _loop.Tick(0.2f);
            Assert.That(_view.Calls, Is.EqualTo(new[] { true, false }));
        }

        [Test]
        public void Disable_DuringProtection_TellsTheViewsItEnded()
        {
            var controller = CreateController();
            controller.Enable();
            RegisteredShip.TakeDamage(1);

            controller.Disable();

            Assert.That(_view.Calls, Is.EqualTo(new[] { true, false }));
        }

        [Test]
        public void Hit_Lethal_TellsTheViewsBeforeTheShipIsReportedDestroyed()
        {
            int? callsWhenReported = null;
            _events.OnDestroyed = () => callsWhenReported = _view.Calls.Count;
            CreateController().Enable();

            RegisteredShip.TakeDamage(3);

            Assert.That(callsWhenReported, Is.EqualTo(2));
            Assert.That(_view.Calls, Is.EqualTo(new[] { true, false }));
        }

        [Test]
        public void Hit_WithoutInvulnerability_NeverTellsTheViews()
        {
            _combat.InvulnerabilitySeconds = 0f;
            CreateController().Enable();

            RegisteredShip.TakeDamage(1);

            Assert.That(_view.Calls, Is.Empty);
        }

        [Test]
        public void WaveStart_ProtectsTheShipLikeAHitWould()
        {
            CreateController().Enable();

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
        public void WaveStart_WhileTheShipIsDisabled_ProtectsNothingLater()
        {
            var controller = CreateController();

            _waveProtection.OnWaveStarted(new WaveStarted(1, 3));
            controller.Enable();
            RegisteredShip.TakeDamage(1);

            Assert.That(_health.Current.Value, Is.EqualTo(2));
        }

        [Test]
        public void WaveStart_AfterTheShipWasDisabled_DoesNotThrowOrProtect()
        {
            var controller = CreateController();
            controller.Enable();
            controller.Disable();

            Assert.DoesNotThrow(() => _waveProtection.OnWaveStarted(new WaveStarted(1, 3)));
            Assert.That(_collider.Ignored, Is.EqualTo(CollisionLayer.None));
            Assert.That(_view.Calls, Is.Empty);
        }

        [Test]
        public void WaveStart_ZeroSeconds_ProtectsNothing()
        {
            _combat.WaveStartProtectionSeconds = 0f;
            CreateController().Enable();

            _waveProtection.OnWaveStarted(new WaveStarted(1, 3));
            RegisteredShip.TakeDamage(1);

            Assert.That(_health.Current.Value, Is.EqualTo(2));
        }

        [Test]
        public void WaveStart_DuringALongerHitWindow_DoesNotShortenIt()
        {
            _combat.InvulnerabilitySeconds = 5f;
            _combat.WaveStartProtectionSeconds = 1f;
            CreateController().Enable();
            RegisteredShip.TakeDamage(1);

            _waveProtection.OnWaveStarted(new WaveStarted(2, 3));
            _loop.Tick(4.9f);
            RegisteredShip.TakeDamage(1);

            Assert.That(_health.Current.Value, Is.EqualTo(2));
        }

        [Test]
        public void Construct_DoesNotAttachTheWeaponUntilEnabled()
        {
            CreateController();

            Assert.That(_weapon.Attachments, Is.Empty);
        }

        [Test]
        public void Enable_AttachesTheWeaponOnceToTheShipWithTheFireInput()
        {
            CreateController().Enable();

            Assert.That(_weapon.Attachments.Count, Is.EqualTo(1));
            Assert.That(_weapon.Attachments[0].Carrier, Is.SameAs(_entity));
            Assert.That(_weapon.Attachments[0].Trigger, Is.SameAs(_input));
        }

        [Test]
        public void Disable_DetachesTheWeapon()
        {
            var controller = CreateController();
            controller.Enable();

            controller.Disable();

            Assert.That(_weapon.LiveCount, Is.EqualTo(0));
        }

        [Test]
        public void Damage_Lethal_DetachesTheWeaponBeforeReportingIt()
        {
            var liveWeaponsWhenReported = -1;
            _events.OnDestroyed = () => liveWeaponsWhenReported = _weapon.LiveCount;
            CreateController().Enable();

            _health.TakeDamage(3);

            Assert.That(liveWeaponsWhenReported, Is.EqualTo(0));
        }

        private sealed class TestInput : IMovementInput, IFireInput
        {
            public float Turn { get; set; }
            public float Thrust { get; set; }
            public bool IsFiring { get; set; }
        }

        private sealed class TestConfig : IShipMovementConfig
        {
            public float Thrust { get; set; }
            public float TurnSpeed { get; set; }
            public float MaxSpeed { get; set; }
            public float LinearDamping { get; set; }
        }
    }
}

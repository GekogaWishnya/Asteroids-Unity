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
        private BodyPose _pose;
        private Entity _entity;
        private PlayerHealth _health;
        private FakeDamageRegistry _registry;
        private RecordingShipEvents _events;

        [SetUp]
        public void SetUp()
        {
            _loop = new FakeSimulationLoop();
            _input = new TestInput();
            _config = new TestConfig { Thrust = 10f, TurnSpeed = 100f, MaxSpeed = 100f, LinearDamping = 0f };
            _pose = new BodyPose();
            _entity = new Entity(_pose);
            _health = new PlayerHealth(new TestHealthConfig { MaxHealth = 3 });
            _registry = new FakeDamageRegistry();
            _events = new RecordingShipEvents();
        }

        private ShipController CreateController() =>
            new(_input, _config, _loop, _entity, _health, _registry, _events);

        [Test]
        public void Enable_RegistersOneStepInInputAndOneInMotion()
        {
            CreateController().Enable();

            Assert.That(_loop.Entries.Count, Is.EqualTo(2));
            Assert.That(_loop.LiveCount(SimulationPhase.Input), Is.EqualTo(1));
            Assert.That(_loop.LiveCount(SimulationPhase.Motion), Is.EqualTo(1));
            Assert.That(_loop.LiveCount(SimulationPhase.Collision), Is.EqualTo(0));
        }

        [Test]
        public void Construct_DoesNotRegisterUntilEnabled()
        {
            CreateController();

            Assert.That(_loop.Entries, Is.Empty);
        }

        [Test]
        public void Disable_DisposesBothRegistrations()
        {
            var controller = CreateController();
            controller.Enable();

            controller.Disable();

            Assert.That(_loop.Entries.Count, Is.EqualTo(2));
            Assert.That(_loop.Entries, Has.All.Matches<FakeSimulationLoop.Entry>(e => e.IsDisposed));
        }

        [Test]
        public void Enable_Twice_RegistersOnce()
        {
            var controller = CreateController();

            controller.Enable();
            controller.Enable();

            Assert.That(_loop.Entries.Count, Is.EqualTo(2));
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

            Assert.That(_loop.Entries.Count, Is.EqualTo(4));
            Assert.That(_loop.LiveCount(SimulationPhase.Input), Is.EqualTo(1));
            Assert.That(_loop.LiveCount(SimulationPhase.Motion), Is.EqualTo(1));
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
            Assert.Throws<ArgumentNullException>(
                () => new ShipController(null, _config, _loop, _entity, _health, _registry, _events));
            Assert.Throws<ArgumentNullException>(
                () => new ShipController(_input, null, _loop, _entity, _health, _registry, _events));
            Assert.Throws<ArgumentNullException>(
                () => new ShipController(_input, _config, null, _entity, _health, _registry, _events));
            Assert.Throws<ArgumentNullException>(
                () => new ShipController(_input, _config, _loop, null, _health, _registry, _events));
            Assert.Throws<ArgumentNullException>(
                () => new ShipController(_input, _config, _loop, _entity, null, _registry, _events));
            Assert.Throws<ArgumentNullException>(
                () => new ShipController(_input, _config, _loop, _entity, _health, null, _events));
            Assert.Throws<ArgumentNullException>(
                () => new ShipController(_input, _config, _loop, _entity, _health, _registry, null));
        }

        [Test]
        public void Enable_RegistersTheShipAsDamageableOfItsEntity()
        {
            CreateController().Enable();

            Assert.That(_registry.LiveDamageables, Is.EqualTo(1));
            Assert.That(_registry.Damageables[0].Entity, Is.SameAs(_entity));
            Assert.That(_registry.Damageables[0].Item, Is.SameAs(_health));
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
            _events.OnDestroyed = () =>
            {
                liveStepsWhenReported = _loop.LiveCount(SimulationPhase.Input) +
                                        _loop.LiveCount(SimulationPhase.Motion);
                liveDamageablesWhenReported = _registry.LiveDamageables;
            };
            CreateController().Enable();

            _health.TakeDamage(3);

            Assert.That(liveStepsWhenReported, Is.EqualTo(0));
            Assert.That(liveDamageablesWhenReported, Is.EqualTo(0));
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

        private sealed class TestInput : IMovementInput
        {
            public float Turn { get; set; }
            public float Thrust { get; set; }
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

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

        [SetUp]
        public void SetUp()
        {
            _loop = new FakeSimulationLoop();
            _input = new TestInput();
            _config = new TestConfig { Thrust = 10f, TurnSpeed = 100f, MaxSpeed = 100f, LinearDamping = 0f };
            _pose = new BodyPose();
        }

        private ShipController CreateController() => new(_input, _config, _loop, _pose);

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
            Assert.Throws<System.ArgumentNullException>(() => new ShipController(null, _config, _loop, _pose));
            Assert.Throws<System.ArgumentNullException>(() => new ShipController(_input, null, _loop, _pose));
            Assert.Throws<System.ArgumentNullException>(() => new ShipController(_input, _config, null, _pose));
            Assert.Throws<System.ArgumentNullException>(() => new ShipController(_input, _config, _loop, null));
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

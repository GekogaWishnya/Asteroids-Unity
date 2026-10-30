using System;
using Asteroids.Core;
using Asteroids.Physics;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Player.Tests
{
    [TestFixture]
    public sealed class ShipMovementTests
    {
        private const float Dt = 0.1f;
        private const float Tolerance = 1e-4f;

        private FakeSimulationLoop _loop;
        private TestShipInput _input;
        private TestMovementConfig _config;
        private BodyPose _pose;
        private Entity _ship;

        [SetUp]
        public void SetUp()
        {
            _loop = new FakeSimulationLoop();
            _input = new TestShipInput();
            _config = new TestMovementConfig { Thrust = 10f, TurnSpeed = 100f, MaxSpeed = 100f };
            _pose = new BodyPose();
            _ship = new Entity(_pose);
        }

        private ShipMovement Create() => new(_input, _config, _loop);

        [Test]
        public void Construct_RegistersNothing()
        {
            Create();

            Assert.That(_loop.Entries, Is.Empty);
        }

        [Test]
        public void Attach_RegistersOneStepInInputAndOneInMotion()
        {
            Create().Attach(_ship);

            Assert.That(_loop.Entries.Count, Is.EqualTo(2));
            Assert.That(_loop.LiveCount(SimulationPhase.Input), Is.EqualTo(1));
            Assert.That(_loop.LiveCount(SimulationPhase.Motion), Is.EqualTo(1));
        }

        [Test]
        public void Dispose_RemovesBothSteps()
        {
            var attachment = Create().Attach(_ship);

            attachment.Dispose();

            Assert.That(_loop.Entries, Has.All.Matches<FakeSimulationLoop.Entry>(e => e.IsDisposed));
        }

        [Test]
        public void Attach_AfterDispose_RegistersAgain()
        {
            var movement = Create();
            movement.Attach(_ship).Dispose();

            movement.Attach(_ship);

            Assert.That(_loop.Entries.Count, Is.EqualTo(4));
            Assert.That(_loop.LiveCount(SimulationPhase.Input), Is.EqualTo(1));
            Assert.That(_loop.LiveCount(SimulationPhase.Motion), Is.EqualTo(1));
        }

        [Test]
        public void Attach_Twice_Throws()
        {
            var movement = Create();
            movement.Attach(_ship);

            Assert.Throws<InvalidOperationException>(() => movement.Attach(_ship));
        }

        [Test]
        public void Tick_WithTurnInput_DecreasesAngleByTurnSpeedTimesDelta()
        {
            _pose.Angle = 90f;
            _input.Turn = 1f;
            Create().Attach(_ship);

            _loop.Tick(Dt);

            Assert.That(_pose.Angle, Is.EqualTo(90f - _config.TurnSpeed * Dt).Within(Tolerance));
        }

        [Test]
        public void Tick_WithThrustInput_MovesAlongUp()
        {
            _input.Thrust = 1f;
            Create().Attach(_ship);

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
            Create().Attach(_ship);

            _loop.Tick(Dt);

            Assert.That(_pose.Position.y, Is.EqualTo(_config.MaxSpeed * Dt).Within(Tolerance));
        }

        [Test]
        public void Tick_AfterDispose_DoesNotMoveOrTurn()
        {
            _input.Turn = 1f;
            _input.Thrust = 1f;
            Create().Attach(_ship).Dispose();

            _loop.Tick(Dt);

            Assert.That(_pose.Angle, Is.EqualTo(0f));
            Assert.That(_pose.Position, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void Attach_AfterDispose_KeepsTheVelocity()
        {
            var movement = Create();
            var attachment = movement.Attach(_ship);
            _input.Thrust = 1f;
            _loop.Tick(Dt);
            _input.Thrust = 0f;
            attachment.Dispose();
            var before = _pose.Position;

            movement.Attach(_ship);
            _loop.Tick(Dt);

            // Velocity after the first tick is Thrust * dt, and it carries on undamped.
            Assert.That(_pose.Position.y - before.y, Is.EqualTo(_config.Thrust * Dt * Dt).Within(Tolerance));
        }

        [Test]
        public void Attach_NullShip_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Create().Attach(null));
        }

        [Test]
        public void Construct_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new ShipMovement(null, _config, _loop));
            Assert.Throws<ArgumentNullException>(() => new ShipMovement(_input, null, _loop));
            Assert.Throws<ArgumentNullException>(() => new ShipMovement(_input, _config, null));
        }
    }
}

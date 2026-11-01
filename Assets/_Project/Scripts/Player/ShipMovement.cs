using System;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Physics;
using Zenject;

namespace Asteroids.Player
{
    /// <summary>
    /// Flies the ship. While attached, the ship is a kinetic body, so it bounces off other bodies it collides
    /// with and gets locked while drifting off.
    /// The motion lives as long as the part, so the ship keeps its velocity when it is disabled and enabled again.
    /// </summary>
    public sealed class ShipMovement : IShipPart
    {
        private readonly IMovementInput _input;
        private readonly IShipMovementConfig _config;
        private readonly ISimulationLoop _loop;
        private readonly IKineticBodyRegistry _bodies;
        private readonly InertialMotion _motion;
        private readonly KineticBody _body;
        private readonly PartAttachment _attachment = new("ship movement");

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public ShipMovement(
            IMovementInput input,
            IShipControlLock controlLock,
            IShipMovementConfig config,
            ISimulationLoop loop,
            IKineticBodyRegistry bodies)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (controlLock == null) throw new ArgumentNullException(nameof(controlLock));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _loop = loop ?? throw new ArgumentNullException(nameof(loop));
            _bodies = bodies ?? throw new ArgumentNullException(nameof(bodies));

            _input = new GatedMovementInput(input, controlLock);
            _motion = new InertialMotion(config);
            _body = new KineticBody(_motion, config);
        }

        /// <summary>Ship's velocity.</summary>
        public IMotionState Motion => _motion;

        public IDisposable Attach(IEntity ship)
        {
            if (ship == null) throw new ArgumentNullException(nameof(ship));

            var pose = ship.Pose;
            var controlStep = new ShipControlStep(_input, _config, pose, _motion);
            var motionStep = new MotionStep(pose, _motion);

            return _attachment.Start(new Func<IDisposable>[]
            {
                () => _loop.Register(SimulationPhase.Input, controlStep),
                () => _loop.Register(SimulationPhase.Motion, motionStep),
                () => _bodies.Register(ship, _body)
            });
        }

        public sealed class Factory : PlaceholderFactory<ShipMovement> { }
    }
}

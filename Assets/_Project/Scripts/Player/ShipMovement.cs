using System;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Physics;
using Zenject;

namespace Asteroids.Player
{
    /// <summary>
    /// Flies the ship.
    /// The motion lives as long as the part, so the ship keeps its velocity when it is disabled and enabled again.
    /// </summary>
    public sealed class ShipMovement : IShipPart
    {
        private readonly IMovementInput _input;
        private readonly IShipMovementConfig _config;
        private readonly ISimulationLoop _loop;
        private readonly InertialMotion _motion;
        private readonly PartAttachment _attachment = new("ship movement");

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public ShipMovement(IMovementInput input, IShipMovementConfig config, ISimulationLoop loop)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _loop = loop ?? throw new ArgumentNullException(nameof(loop));
            _motion = new InertialMotion(config);
        }

        public IDisposable Attach(IEntity ship)
        {
            if (ship == null) throw new ArgumentNullException(nameof(ship));

            var pose = ship.Pose;
            var controlStep = new ShipControlStep(_input, _config, pose, _motion);
            var motionStep = new MotionStep(pose, _motion);

            return _attachment.Start(new Func<IDisposable>[]
            {
                () => _loop.Register(SimulationPhase.Input, controlStep),
                () => _loop.Register(SimulationPhase.Motion, motionStep)
            });
        }

        public sealed class Factory : PlaceholderFactory<ShipMovement> { }
    }
}

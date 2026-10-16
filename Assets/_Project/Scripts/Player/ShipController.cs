using System;
using Asteroids.Common;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Physics;

namespace Asteroids.Player
{
    /// <summary>
    /// Assembles the behavior of a ship.
    /// Creates and registers <see cref="ShipControlStep"/> in <see cref="SimulationPhase.Input"/> and
    /// <see cref="MotionStep"/> in <see cref="SimulationPhase.Motion"/>.
    /// Both are sharing one <see cref="InertialMotion"/>.
    /// </summary>
    public sealed class ShipController : IShipController
    {
        private readonly RegistrationScope _registrations;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public ShipController(IMovementInput input, IShipMovementConfig config, ISimulationLoop loop, IPose pose)
        {
            if (loop == null) throw new ArgumentNullException(nameof(loop));

            var motion = new InertialMotion(config);
            var controlStep = new ShipControlStep(input, config, pose, motion);
            var motionStep = new MotionStep(pose, motion);

            _registrations = new RegistrationScope(new Func<IDisposable>[]
            {
                () => loop.Register(SimulationPhase.Input, controlStep),
                () => loop.Register(SimulationPhase.Motion, motionStep)
            });
        }

        public void Enable() => _registrations.Enable();
        public void Disable() => _registrations.Disable();
    }
}

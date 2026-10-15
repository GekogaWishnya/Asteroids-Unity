using System;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Physics;

namespace Asteroids.Player
{
    /// <summary>Turns player input into rotation and acceleration.</summary>
    public sealed class ShipControlStep : ISimulationStep
    {
        private readonly IMovementInput _input;
        private readonly IShipMovementConfig _config;
        private readonly IPose _pose;
        private readonly IMotionDriver _driver;

        public ShipControlStep(IMovementInput input, IShipMovementConfig config, IPose pose, IMotionDriver driver)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _pose = pose ?? throw new ArgumentNullException(nameof(pose));
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
        }

        public void Tick(float deltaTime)
        {
            _pose.Angle -= _input.Turn * _config.TurnSpeed * deltaTime;
            _driver.Accelerate(_pose.Up * (_input.Thrust * _config.Thrust));
        }
    }
}

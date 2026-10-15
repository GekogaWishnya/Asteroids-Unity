using System;
using Asteroids.Core;

namespace Asteroids.Physics
{
    /// <summary>Advances <see cref="InertialMotion"/> and writes the displacement into the pose.</summary>
    public sealed class MotionStep : ISimulationStep
    {
        private readonly IPose _pose;
        private readonly InertialMotion _motion;

        public MotionStep(IPose pose, InertialMotion motion)
        {
            _pose = pose ?? throw new ArgumentNullException(nameof(pose));
            _motion = motion ?? throw new ArgumentNullException(nameof(motion));
        }

        public void Tick(float deltaTime) => _pose.Position += _motion.Step(deltaTime);
    }
}

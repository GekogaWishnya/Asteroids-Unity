using System;
using Asteroids.Core;

namespace Asteroids.Physics
{
    /// <summary>Advances <see cref="AngularMotion"/> and writes the rotation into the pose.</summary>
    public sealed class SpinStep : ISimulationStep
    {
        private readonly IPose _pose;
        private readonly AngularMotion _motion;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public SpinStep(IPose pose, AngularMotion motion)
        {
            _pose = pose ?? throw new ArgumentNullException(nameof(pose));
            _motion = motion ?? throw new ArgumentNullException(nameof(motion));
        }

        public void Tick(float deltaTime) => _pose.Angle += _motion.Step(deltaTime);
    }
}

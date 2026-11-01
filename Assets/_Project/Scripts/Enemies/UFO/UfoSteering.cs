using System;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Physics;
using UnityEngine;

namespace Asteroids.Enemies
{
    /// <summary>
    /// <para>
    /// Moves a UFO like a homing rocket. Every tick it turns its nose toward the player's ship by at most
    /// <see cref="IUfoTypeConfig.TurnSpeed"/> and thrusts, so the ship can dodge it by turning tighter than the UFO can.
    /// </para>
    /// <para>
    /// Without a target it neither turns nor thrusts and drifts on its inertia.
    /// While stunned it does the same, and the stun counts down.
    /// </para>
    /// </summary>
    public sealed class UfoSteering : ISimulationStep, IStunnable
    {
        private const float MinAimDistanceSqr = 1e-6f;

        private readonly IUfoTypeConfig _config;
        private readonly IPose _pose;
        private readonly IMotionDriver _driver;
        private readonly IPlayerTarget _target;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public UfoSteering(IUfoTypeConfig config, IPose pose, IMotionDriver driver, IPlayerTarget target)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _pose = pose ?? throw new ArgumentNullException(nameof(pose));
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _target = target ?? throw new ArgumentNullException(nameof(target));
        }
        
        public float StunRemaining { get; private set; }

        public bool IsStunned => StunRemaining > 0f;

        public void Stun(float seconds)
        {
            if (!float.IsFinite(seconds) || seconds < 0f)
                throw new ArgumentOutOfRangeException(
                    nameof(seconds), "Duration must be a finite number that is not negative.");

            StunRemaining = Math.Max(StunRemaining, seconds);
        }
        
        public void Reset() => StunRemaining = 0f;

        public void Tick(float deltaTime)
        {
            if (IsStunned)
            {
                StunRemaining = Math.Max(0f, StunRemaining - deltaTime);
                return;
            }

            if (!_target.HasTarget) return;

            var toTarget = _target.Position - _pose.Position;
            if (toTarget.sqrMagnitude > MinAimDistanceSqr)
                _pose.Angle = Mathf.MoveTowardsAngle(_pose.Angle, AngleOf(toTarget), _config.TurnSpeed * deltaTime);

            _driver.Accelerate(_pose.Up * _config.Thrust);
        }
        
        public static float AngleOf(Vector2 direction) => Mathf.Atan2(-direction.x, direction.y) * Mathf.Rad2Deg;
    }
}

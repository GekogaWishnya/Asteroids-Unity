using System;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Physics
{
    public sealed class KineticBody : IKineticBody
    {
        private readonly IMotionState _state;
        private readonly IMotionDriver _driver;
        private readonly IKnockbackConfig _config;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public KineticBody(IMotionState state, IMotionDriver driver, IKnockbackConfig config)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }
        
        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public KineticBody(InertialMotion motion, IKnockbackConfig config) : this(motion, motion, config) { }

        public Vector2 Velocity => _state.Velocity;
        
        public float MinKnockbackSpeed
        {
            get
            {
                var speed = _config.MinKnockbackSpeed;
                return float.IsFinite(speed) && speed > 0f ? speed : 0f;
            }
        }

        public void AddVelocity(Vector2 delta) => _driver.AddVelocity(delta);
    }
}
using System;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Health;
using Asteroids.Physics;
using UnityEngine;

namespace Asteroids.Enemies
{
    /// <summary>
    /// Lifecycle of one UFO object, reused for many lives. <see cref="Launch"/> starts a life.
    /// </summary>
    public sealed class UfoController : IDamageSource, IDisposable
    {
        private readonly IUfoTypeConfig _config;
        private readonly InertialMotion _motion;
        private readonly UfoSteering _steering;
        private readonly EnemyLife _life;

        /// <exception cref="ArgumentNullException">Any argument (or the view's entity) is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The config's max health is not positive or its stun is invalid.</exception>
        public UfoController(
            IEnemyView view,
            IUfoTypeConfig config,
            IPlayerTarget target,
            ISimulationLoop loop,
            IDamageRegistry damageRegistry,
            IKineticBodyRegistry bodyRegistry,
            IEnemyDeathHandler death)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            if (death == null) throw new ArgumentNullException(nameof(death));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (loop == null) throw new ArgumentNullException(nameof(loop));
            if (damageRegistry == null) throw new ArgumentNullException(nameof(damageRegistry));
            if (bodyRegistry == null) throw new ArgumentNullException(nameof(bodyRegistry));

            var entity = view.Entity ?? throw new ArgumentNullException(nameof(view), "The view has no entity.");

            var health = new HealthModel(config.MaxHealth);
            _motion = new InertialMotion(config);
            _steering = new UfoSteering(config, entity.Pose, _motion, target);

            var motionStep = new MotionStep(entity.Pose, _motion);
            var body = new StunOnKnockback(new KineticBody(_motion, config), _steering, config.StunSeconds);

            _life = new EnemyLife("UFO", view, health, death, new Func<IDisposable>[]
            {
                () => loop.Register(SimulationPhase.Input, _steering),
                () => loop.Register(SimulationPhase.Motion, motionStep),
                () => damageRegistry.RegisterDamageable(entity, health),
                () => damageRegistry.RegisterSource(entity, this),
                () => bodyRegistry.Register(entity, body)
            });
            _life.Finished += () => Finished?.Invoke(this);
        }

        /// <summary>Raised once per life, when the UFO is out of play again and can be reused.</summary>
        public event Action<UfoController> Finished;

        public bool IsInPlay => _life.IsInPlay;

        public bool IsStunned => _steering.IsStunned;
        
        public int Damage => _config.ContactDamage;

        /// <summary>
        /// Puts the UFO in play.
        /// </summary>
        /// <exception cref="ArgumentException"><paramref name="direction"/> has no length or is not finite.</exception>
        /// <exception cref="InvalidOperationException">The UFO is already in play.</exception>
        public void Launch(Vector2 position, Vector2 direction)
        {
            var length = direction.magnitude;
            if (!float.IsFinite(length) || length <= 0f)
                throw new ArgumentException("The direction must be a finite vector with a length.", nameof(direction));
            if (_life.IsInPlay) throw new InvalidOperationException("The UFO is already in play.");

            var heading = direction / length;

            _steering.Reset();
            _life.Enter(position, UfoSteering.AngleOf(heading),
                () => _motion.AddVelocity(heading * _config.LaunchSpeed - _motion.Velocity));
        }
        
        public void Dispose()
        {
            Finished = null;
            _life.Dispose();
        }
    }
}

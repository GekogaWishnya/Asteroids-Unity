using System;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Health;
using Asteroids.Physics;

namespace Asteroids.Enemies
{
    /// <summary>
    /// <para>
    /// Lifecycle of one asteroid object, reused for many lives. <see cref="Launch"/> starts a life cycle. Object
    /// is shown, its health is refilled, and inertial motion (<see cref="MotionStep"/>) and a spin
    /// (<see cref="SpinStep"/>) are registered in <see cref="SimulationPhase.Motion"/> together with the
    /// asteroid's health as damageable, the asteroid itself as damage source and its motion as a kinetic body.
    /// The spin and the motion are independent, so spinning never changes the direction of travel.
    /// </para>
    /// <para>
    /// When the health runs out, the asteroid tells its <see cref="IEnemyDeathHandler"/> where it died,
    /// takes itself out of play and then raises <see cref="Finished"/>. It leaves play even if the handler
    /// throws. What death results in (points, fragments) is the handler's business, not the controller's.
    /// </para>
    /// </summary>
    public sealed class AsteroidController : IDamageSource, IDisposable
    {
        private readonly IAsteroidTypeConfig _config;
        private readonly InertialMotion _motion;
        private readonly AngularMotion _spin = new();
        private readonly EnemyLife _life;

        /// <exception cref="ArgumentNullException">Any argument (or the view's entity) is null.</exception>
        public AsteroidController(
            IEnemyView view,
            IAsteroidTypeConfig config,
            ISimulationLoop loop,
            IDamageRegistry damageRegistry,
            IKineticBodyRegistry bodyRegistry,
            IEnemyDeathHandler death)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            if (death == null) throw new ArgumentNullException(nameof(death));
            if (loop == null) throw new ArgumentNullException(nameof(loop));
            if (damageRegistry == null) throw new ArgumentNullException(nameof(damageRegistry));
            if (bodyRegistry == null) throw new ArgumentNullException(nameof(bodyRegistry));

            var entity = view.Entity ?? throw new ArgumentNullException(nameof(view), "The view has no entity.");

            var health = new HealthModel(config.MaxHealth);
            _motion = new InertialMotion(new DriftConfig(config.MaxSpeed));

            var motionStep = new MotionStep(entity.Pose, _motion);
            var spinStep = new SpinStep(entity.Pose, _spin);
            var body = new KineticBody(_motion, DriftConfig.NoKnockback);

            _life = new EnemyLife("asteroid", view, health, death, new Func<IDisposable>[]
            {
                () => loop.Register(SimulationPhase.Motion, motionStep),
                () => loop.Register(SimulationPhase.Motion, spinStep),
                () => damageRegistry.RegisterDamageable(entity, health),
                () => damageRegistry.RegisterSource(entity, this),
                () => bodyRegistry.Register(entity, body)
            });
            _life.Finished += () => Finished?.Invoke(this);
        }

        /// <summary>Raised once per life, when the asteroid is out of play again and can be reused.</summary>
        public event Action<AsteroidController> Finished;

        public bool IsInPlay => _life.IsInPlay;

        /// <summary>Damage dealt to whatever the asteroid touches and may hurt.</summary>
        public int Damage => _config.ContactDamage;

        /// <exception cref="InvalidOperationException">The asteroid is already in play.</exception>
        public void Launch(AsteroidLaunch launch) =>
            _life.Enter(launch.Position, launch.Angle, () =>
            {
                _motion.AddVelocity(launch.Velocity - _motion.Velocity);
                _spin.Velocity = launch.SpinSpeed;
            });

        /// <summary>Takes the asteroid out of play if needed and destroys its object. Not reusable afterwards.</summary>
        public void Dispose()
        {
            Finished = null;
            _life.Dispose();
        }

        private sealed class DriftConfig : IMotionConfig, IKnockbackConfig
        {
            public static readonly IKnockbackConfig NoKnockback = new DriftConfig(0f);

            public DriftConfig(float maxSpeed) => MaxSpeed = maxSpeed;

            public float MaxSpeed { get; }
            public float LinearDamping => 0f;
            public float MinKnockbackSpeed => 0f;
        }
    }
}

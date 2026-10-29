using System;
using Asteroids.Common;
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
    /// asteroid's health as damageable and the asteroid itself as damage source. The two motions are
    /// independent, so spinning never changes the direction of travel.
    /// </para>
    /// <para>
    /// When the health runs out, the asteroid tells its <see cref="IEnemyDeathHandler"/> where it died,
    /// takes itself out of play and then raises <see cref="Finished"/>. It leaves play even if the handler
    /// throws. What death results in (points, fragments) is the handler's business, not the controller's.
    /// </para>
    /// </summary>
    public sealed class AsteroidController : IDamageSource, IDisposable
    {
        private readonly IEnemyView _view;
        private readonly IAsteroidTypeConfig _config;
        private readonly IEnemyDeathHandler _death;
        private readonly HealthModel _health;
        private readonly InertialMotion _motion;
        private readonly AngularMotion _spin = new();
        private readonly RegistrationScope _registrations;

        /// <exception cref="ArgumentNullException">Any argument (or the view's entity) is null.</exception>
        public AsteroidController(
            IEnemyView view,
            IAsteroidTypeConfig config,
            ISimulationLoop loop,
            IDamageRegistry damageRegistry,
            IEnemyDeathHandler death)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _death = death ?? throw new ArgumentNullException(nameof(death));
            if (loop == null) throw new ArgumentNullException(nameof(loop));
            if (damageRegistry == null) throw new ArgumentNullException(nameof(damageRegistry));

            var entity = view.Entity ?? throw new ArgumentNullException(nameof(view), "The view has no entity.");

            _health = new HealthModel(config.MaxHealth);
            _motion = new InertialMotion(new DriftConfig(config.MaxSpeed));

            var motionStep = new MotionStep(entity.Pose, _motion);
            var spinStep = new SpinStep(entity.Pose, _spin);

            _registrations = new RegistrationScope(new Func<IDisposable>[]
            {
                () => loop.Register(SimulationPhase.Motion, motionStep),
                () => loop.Register(SimulationPhase.Motion, spinStep),
                () => damageRegistry.RegisterDamageable(entity, _health),
                () => damageRegistry.RegisterSource(entity, this),
                ListenForDepletion
            });
        }

        /// <summary>Raised once per life, when the asteroid is out of play again and can be reused.</summary>
        public event Action<AsteroidController> Finished;

        public bool IsInPlay => _registrations.IsEnabled;

        /// <summary>Damage dealt to whatever the asteroid touches and may hurt.</summary>
        public int Damage => _config.ContactDamage;

        /// <exception cref="InvalidOperationException">The asteroid is already in play.</exception>
        public void Launch(AsteroidLaunch launch)
        {
            if (_registrations.IsEnabled)
                throw new InvalidOperationException("The asteroid is already in play.");

            _health.Reset();
            _registrations.Enable();

            try
            {
                _view.Show(launch.Position, launch.Angle);
                
                _motion.ApplyImpulse(launch.Velocity - _motion.Velocity);
                _spin.Velocity = launch.SpinSpeed;
            }
            catch
            {
                try
                {
                    Leave();
                }
                catch
                {
                    // The failure that led here matters more than a failure of the cleanup.
                }

                throw;
            }
        }

        /// <summary>Takes the asteroid out of play if needed and destroys its object. Not reusable afterwards.</summary>
        public void Dispose()
        {
            Finished = null;

            try
            {
                _registrations.Dispose();
            }
            finally
            {
                _view.Dispose();
            }
        }

        private IDisposable ListenForDepletion()
        {
            _health.Depleted += OnDepleted;
            return new DepletionSubscription(_health, OnDepleted);
        }

        private void OnDepleted()
        {
            var position = _view.Entity.Pose.Position;

            try
            {
                _death.Died(position);
            }
            finally
            {
                Retire();
            }
        }

        private void Retire()
        {
            try
            {
                Leave();
            }
            finally
            {
                Finished?.Invoke(this);
            }
        }

        private void Leave()
        {
            try
            {
                _registrations.Disable();
            }
            finally
            {
                _view.Hide();
            }
        }

        private sealed class DriftConfig : IMotionConfig
        {
            public DriftConfig(float maxSpeed) => MaxSpeed = maxSpeed;

            public float MaxSpeed { get; }
            public float LinearDamping => 0f;
        }

        private sealed class DepletionSubscription : IDisposable
        {
            private readonly HealthModel _health;
            private readonly Action _handler;

            public DepletionSubscription(HealthModel health, Action handler)
            {
                _health = health;
                _handler = handler;
            }

            public void Dispose() => _health.Depleted -= _handler;
        }
    }
}

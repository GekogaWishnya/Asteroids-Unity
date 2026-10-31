using System;
using Asteroids.Common;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Physics;

namespace Asteroids.Weapons.Projectiles
{
    /// <summary>
    /// Lifecycle of one projectile object, reused for many shots. <see cref="Launch"/> shows the object and
    /// registers its <see cref="MotionStep"/> in <see cref="SimulationPhase.Motion"/>, its
    /// lifetime in <see cref="SimulationPhase.Logic"/>, the projectile as a damage source and the projectile
    /// itself in the <see cref="IProjectileRegistry"/> so hits can find it.
    /// </summary>
    public sealed class Projectile : IDamageSource, IProjectileImpact, IDisposable
    {
        private readonly IProjectileView _view;
        private readonly IProjectileWeaponConfig _config;
        private readonly InertialMotion _motion;
        private readonly RegistrationScope _registrations;
        private float _remaining;
        private bool _hasHit;

        /// <exception cref="ArgumentNullException">Any argument (or the view's entity) is null.</exception>
        public Projectile(
            IProjectileView view,
            IProjectileWeaponConfig config,
            ISimulationLoop loop,
            IDamageRegistry damageRegistry,
            IProjectileRegistry projectiles)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            if (loop == null) throw new ArgumentNullException(nameof(loop));
            if (damageRegistry == null) throw new ArgumentNullException(nameof(damageRegistry));
            if (projectiles == null) throw new ArgumentNullException(nameof(projectiles));

            var entity = view.Entity ?? throw new ArgumentNullException(nameof(view), "The view has no entity.");

            _motion = new InertialMotion(new StraightFlight(config));
            var motionStep = new MotionStep(entity.Pose, _motion);
            var lifetimeStep = new LifetimeStep(this);

            _registrations = new RegistrationScope(new Func<IDisposable>[]
            {
                () => loop.Register(SimulationPhase.Motion, motionStep),
                () => loop.Register(SimulationPhase.Logic, lifetimeStep),
                () => damageRegistry.RegisterSource(entity, this),
                () => projectiles.Register(entity, this)
            });
        }

        /// <summary>Raised once per shot, when the projectile is out of play again and can be reused.</summary>
        public event Action<Projectile> Finished;

        public bool IsInPlay => _registrations.IsEnabled;
        
        public int Damage => _config.ProjectileDamage;

        /// <exception cref="InvalidOperationException">The projectile is already in play.</exception>
        public void Launch(ProjectileLaunch launch)
        {
            if (_registrations.IsEnabled)
                throw new InvalidOperationException("The projectile is already in play.");

            _remaining = _config.ProjectileLifetime;
            _hasHit = false;
            _registrations.Enable();

            try
            {
                _view.Show(launch.Position, launch.Angle);
                _motion.AddVelocity(launch.Velocity - _motion.Velocity);
            }
            catch
            {
                try
                {
                    Leave();
                }
                catch
                {
                    // The failure that led here matters more.
                }

                throw;
            }
        }
        
        public void Impact()
        {
            if (IsInPlay) _hasHit = true;
        }
        
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

        private void Tick(float deltaTime)
        {
            _remaining -= deltaTime;
            if (_hasHit || _remaining <= 0f) Retire();
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

        /// <summary>No damping, and the speed cap is the projectile speed.</summary>
        private sealed class StraightFlight : IMotionConfig
        {
            private readonly IProjectileWeaponConfig _config;

            public StraightFlight(IProjectileWeaponConfig config) => _config = config;

            public float MaxSpeed => _config.ProjectileSpeed;
            public float LinearDamping => 0f;
        }

        private sealed class LifetimeStep : ISimulationStep
        {
            private readonly Projectile _owner;

            public LifetimeStep(Projectile owner) => _owner = owner;

            public void Tick(float deltaTime) => _owner.Tick(deltaTime);
        }
    }
}

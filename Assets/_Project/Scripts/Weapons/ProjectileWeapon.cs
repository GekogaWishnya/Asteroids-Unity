using System;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Weapons.Projectiles;

namespace Asteroids.Weapons
{
    /// <summary>
    /// A gun that fires pooled projectiles forward. The new projectile starts moving on the
    /// next tick. Velocity of the carrier is not inherited.
    /// </summary>
    public sealed class ProjectileWeapon : IWeapon
    {
        private readonly IProjectileWeaponConfig _config;
        private readonly ISimulationLoop _loop;
        private readonly ProjectilePool _pool;
        private readonly FireCooldown _cooldown = new();
        private Attachment _attachment;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public ProjectileWeapon(IProjectileWeaponConfig config, ISimulationLoop loop, ProjectilePool pool)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _loop = loop ?? throw new ArgumentNullException(nameof(loop));
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
        }

        public IDisposable Attach(IEntity carrier, IFireInput trigger)
        {
            if (carrier == null) throw new ArgumentNullException(nameof(carrier));
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));
            if (_attachment != null) throw new InvalidOperationException("The weapon is already attached.");

            var step = new TriggerStep(this, carrier.Pose, trigger);
            return _attachment = new Attachment(this, _loop.Register(SimulationPhase.Logic, step));
        }

        private void Fire(IPose pose)
        {
            var direction = pose.Up;
            _pool.Launch(new ProjectileLaunch(
                pose.Position + direction * _config.MuzzleOffset,
                pose.Angle,
                direction * _config.ProjectileSpeed));
        }

        private sealed class TriggerStep : ISimulationStep
        {
            private readonly ProjectileWeapon _weapon;
            private readonly IPose _pose;
            private readonly IFireInput _trigger;

            public TriggerStep(ProjectileWeapon weapon, IPose pose, IFireInput trigger)
            {
                _weapon = weapon;
                _pose = pose ?? throw new ArgumentNullException(nameof(pose), "The carrier has no pose.");
                _trigger = trigger;
            }

            public void Tick(float deltaTime)
            {
                if (_weapon._cooldown.Tick(deltaTime, _trigger.IsFiring, _weapon._config.FireInterval))
                    _weapon.Fire(_pose);
            }
        }

        private sealed class Attachment : IDisposable
        {
            private readonly ProjectileWeapon _owner;
            private readonly IDisposable _step;
            private bool _disposed;

            public Attachment(ProjectileWeapon owner, IDisposable step)
            {
                _owner = owner;
                _step = step;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;

                try
                {
                    _step.Dispose();
                }
                finally
                {
                    if (ReferenceEquals(_owner._attachment, this)) _owner._attachment = null;
                }
            }
        }
    }
}

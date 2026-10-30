using System;
using Asteroids.Common;
using Asteroids.Core;
using Asteroids.Core.IConfigs;

namespace Asteroids.Weapons.Projectiles
{
    public sealed class ProjectilePool
    {
        private readonly IProjectileWeaponConfig _config;
        private readonly IProjectileViewFactory _views;
        private readonly ISimulationLoop _loop;
        private readonly IDamageRegistry _damageRegistry;
        private readonly IProjectileRegistry _projectiles;
        private readonly ObjectPool<Projectile> _pool;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The config's pool max idle is negative.</exception>
        public ProjectilePool(
            IProjectileWeaponConfig config,
            IProjectileViewFactory views,
            ISimulationLoop loop,
            IDamageRegistry damageRegistry,
            IProjectileRegistry projectiles)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _views = views ?? throw new ArgumentNullException(nameof(views));
            _loop = loop ?? throw new ArgumentNullException(nameof(loop));
            _damageRegistry = damageRegistry ?? throw new ArgumentNullException(nameof(damageRegistry));
            _projectiles = projectiles ?? throw new ArgumentNullException(nameof(projectiles));

            _pool = new ObjectPool<Projectile>(CreateInstance, onDestroy: DestroyInstance, maxIdle: config.PoolMaxIdle);
        }
        
        public int InPlayCount => _pool.CountActive;
        
        public void Prewarm() => _pool.Prewarm(_config.PoolPrewarm);

        public void Launch(ProjectileLaunch launch)
        {
            var projectile = _pool.Get();

            try
            {
                projectile.Launch(launch);
            }
            catch
            {
                _pool.Release(projectile);
                throw;
            }
        }

        private Projectile CreateInstance()
        {
            var projectile = new Projectile(_views.Create(), _config, _loop, _damageRegistry, _projectiles);
            projectile.Finished += OnFinished;
            return projectile;
        }

        private void DestroyInstance(Projectile projectile)
        {
            projectile.Finished -= OnFinished;
            projectile.Dispose();
        }

        private void OnFinished(Projectile projectile) => _pool.Release(projectile);
    }
}

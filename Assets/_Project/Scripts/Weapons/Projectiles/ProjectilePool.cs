using System;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Pooling;

namespace Asteroids.Weapons.Projectiles
{
    public sealed class ProjectilePool
    {
        private readonly IProjectileWeaponConfig _config;
        private readonly IProjectileViewFactory _views;
        private readonly ISimulationLoop _loop;
        private readonly IWrapRegistry _wrap;
        private readonly IDamageRegistry _damageRegistry;
        private readonly IProjectileRegistry _projectiles;
        private readonly TrackedPool<Projectile> _pool;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The config's pool max idle is negative.</exception>
        public ProjectilePool(
            IProjectileWeaponConfig config,
            IProjectileViewFactory views,
            ISimulationLoop loop,
            IWrapRegistry wrap,
            IDamageRegistry damageRegistry,
            IProjectileRegistry projectiles)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _views = views ?? throw new ArgumentNullException(nameof(views));
            _loop = loop ?? throw new ArgumentNullException(nameof(loop));
            _wrap = wrap ?? throw new ArgumentNullException(nameof(wrap));
            _damageRegistry = damageRegistry ?? throw new ArgumentNullException(nameof(damageRegistry));
            _projectiles = projectiles ?? throw new ArgumentNullException(nameof(projectiles));

            _pool = new TrackedPool<Projectile>(CreateInstance, config.PoolMaxIdle);
        }

        public int InPlayCount => _pool.InPlayCount;

        public void Prewarm() => _pool.Prewarm(_config.PoolPrewarm);

        public void Launch(ProjectileLaunch launch) =>
            _pool.Spawn(launch, static (projectile, l) => projectile.Launch(l));

        /// <inheritdoc cref="TrackedPool{T}.DespawnAll"/>
        public void DespawnAll() => _pool.DespawnAll();

        private Projectile CreateInstance() =>
            new(_views.Create(), _config, _loop, _wrap, _damageRegistry, _projectiles);
    }
}
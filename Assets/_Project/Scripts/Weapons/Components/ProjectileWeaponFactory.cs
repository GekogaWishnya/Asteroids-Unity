using System;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Weapons.Projectiles;
using Zenject;

namespace Asteroids.Weapons.Components
{
    public sealed class ProjectileWeaponFactory : WeaponFactory<ProjectileWeaponDefinition>
    {
        private readonly ISimulationLoop _loop;
        private readonly IWrapRegistry _wrap;
        private readonly IDamageRegistry _damageRegistry;
        private readonly IProjectileRegistry _projectiles;
        private readonly IInstantiator _instantiator;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public ProjectileWeaponFactory(
            ISimulationLoop loop,
            IWrapRegistry wrap,
            IDamageRegistry damageRegistry,
            IProjectileRegistry projectiles,
            IInstantiator instantiator)
        {
            _loop = loop ?? throw new ArgumentNullException(nameof(loop));
            _wrap = wrap ?? throw new ArgumentNullException(nameof(wrap));
            _damageRegistry = damageRegistry ?? throw new ArgumentNullException(nameof(damageRegistry));
            _projectiles = projectiles ?? throw new ArgumentNullException(nameof(projectiles));
            _instantiator = instantiator ?? throw new ArgumentNullException(nameof(instantiator));
        }

        protected override IWeapon Create(ProjectileWeaponDefinition definition)
        {
            ConfigValidation.ThrowIfInvalid($"Projectile weapon '{definition.name}'", definition.FindProblems());

            var views = new PrefabProjectileViewFactory(_instantiator, definition);
            var pool = new ProjectilePool(definition, views, _loop, _wrap, _damageRegistry, _projectiles);

            pool.Prewarm();
            return new ProjectileWeapon(definition, _loop, pool);
        }
    }
}
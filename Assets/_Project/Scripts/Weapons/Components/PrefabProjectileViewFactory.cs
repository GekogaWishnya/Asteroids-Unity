using System;
using Asteroids.Core;
using Asteroids.Weapons.Projectiles;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Asteroids.Weapons.Components
{
    /// <summary>
    /// Makes the projectile objects of one weapon from its prefab through the container so their components
    /// are injected. New objects are placed under an inactive root and are not awake until they are shown.
    /// The prefab is read from the definition on every call, so a changed prefab applies to every object made
    /// after the change.
    /// </summary>
    public sealed class PrefabProjectileViewFactory : IProjectileViewFactory
    {
        private readonly DiContainer _container;
        private readonly ProjectileWeaponDefinition _definition;
        private Transform _idleRoot;
        private Transform _activeRoot;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public PrefabProjectileViewFactory(DiContainer container, ProjectileWeaponDefinition definition)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _definition = definition ? definition : throw new ArgumentNullException(nameof(definition));
        }

        /// <exception cref="InvalidOperationException">
        /// The definition has no prefab or the prefab has no component implementing <see cref="IEntityProvider"/>.
        /// </exception>
        public IProjectileView Create()
        {
            var prefab = _definition.ProjectilePrefab;
            if (prefab == null)
                throw new InvalidOperationException($"Projectile weapon '{_definition.name}' has no projectile prefab.");

            EnsureRoots();

            var instance = _container.InstantiatePrefab(prefab, _idleRoot);
            if (instance.TryGetComponent<IEntityProvider>(out var provider))
                return new PooledProjectileView(instance.transform, provider.Entity, _idleRoot, _activeRoot);

            Object.Destroy(instance);
            throw new InvalidOperationException(
                $"The projectile prefab '{prefab.name}' of weapon '{_definition.name}' needs a component " +
                $"implementing {nameof(IEntityProvider)} (add EntityComponent).");
        }

        private void EnsureRoots()
        {
            if (_idleRoot != null && _activeRoot != null) return;

            var idle = new GameObject($"Projectile pool of {_definition.name} (idle)");
            idle.SetActive(false);
            _idleRoot = idle.transform;

            _activeRoot = new GameObject($"Projectiles of {_definition.name}").transform;
        }
    }
}

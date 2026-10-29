using System;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using UnityEngine;
using Zenject;

namespace Asteroids.Enemies.Components
{
    /// <summary>
    /// Makes enemy objects from prefabs through the container, so their components are injected.
    /// New objects are placed under an inactive root and are not awake until they are shown.
    /// The prefab must have a component implementing <see cref="IEntityProvider"/>.
    /// </summary>
    public sealed class PrefabEnemyViewFactory : IEnemyViewFactory
    {
        private readonly DiContainer _container;
        private readonly EnemyPrefabCatalog _prefabs;
        private Transform _idleRoot;
        private Transform _activeRoot;

        public PrefabEnemyViewFactory(DiContainer container, EnemyPrefabCatalog prefabs)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _prefabs = prefabs ?? throw new ArgumentNullException(nameof(prefabs));
        }

        public IEnemyView Create(EnemyTypeId type)
        {
            var prefab = _prefabs.Get(type);
            EnsureRoots();

            var instance = _container.InstantiatePrefab(prefab, _idleRoot);
            if (instance.TryGetComponent<IEntityProvider>(out var provider))
                return new PooledObjectView(instance.transform, provider.Entity, _idleRoot, _activeRoot);
            
            UnityEngine.Object.Destroy(instance);
            throw new InvalidOperationException(
                $"The prefab of enemy type '{type}' needs a component implementing " +
                $"{nameof(IEntityProvider)} (add EntityComponent).");

        }

        private void EnsureRoots()
        {
            if (_idleRoot != null && _activeRoot != null) return;

            var idle = new GameObject("Enemy pool (idle)");
            idle.SetActive(false);
            _idleRoot = idle.transform;

            _activeRoot = new GameObject("Enemies").transform;
        }
    }
}

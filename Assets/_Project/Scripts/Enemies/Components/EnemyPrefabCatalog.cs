using System;
using System.Collections.Generic;
using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Enemies.Components
{
    /// <summary>Which prefab an enemy type is made of.</summary>
    public sealed class EnemyPrefabCatalog
    {
        private readonly Dictionary<EnemyTypeId, GameObject> _prefabs = new();

        /// <exception cref="ArgumentNullException"><paramref name="prefabs"/> is null.</exception>
        /// <exception cref="ArgumentException">A type is not valid, has no prefab, or appears twice.</exception>
        public EnemyPrefabCatalog(IEnumerable<KeyValuePair<EnemyTypeId, GameObject>> prefabs)
        {
            if (prefabs == null) throw new ArgumentNullException(nameof(prefabs));

            foreach (var pair in prefabs)
            {
                if (!pair.Key.IsValid)
                    throw new ArgumentException("An enemy type definition has no id.", nameof(prefabs));
                if (pair.Value == null)
                    throw new ArgumentException($"Enemy type '{pair.Key}' has no prefab.", nameof(prefabs));
                if (!_prefabs.TryAdd(pair.Key, pair.Value))
                    throw new ArgumentException(
                        $"Enemy type '{pair.Key}' is defined more than once.", nameof(prefabs));
            }
        }

        /// <exception cref="InvalidOperationException">The type is not in the catalog.</exception>
        public GameObject Get(EnemyTypeId type)
        {
            if (!type.IsValid || !_prefabs.TryGetValue(type, out var prefab))
                throw new InvalidOperationException(
                    $"No prefab is known for enemy type '{type}'. " +
                    "Add its enemy type definition to the list on the gameplay installer.");

            return prefab;
        }
    }
}

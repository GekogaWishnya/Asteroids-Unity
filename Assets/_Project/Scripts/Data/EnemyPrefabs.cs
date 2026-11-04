using System;
using System.Collections.Generic;
using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Data
{
    public static class EnemyPrefabs
    {
        /// <summary>Every enemy type the sources refer to with its prefab.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="sources"/> or one of them is null.</exception>
        /// <exception cref="InvalidOperationException">A type has no name, or two different types share a name.</exception>
        public static List<KeyValuePair<EnemyTypeId, GameObject>> Collect(IEnumerable<IEnemyTypeSource> sources)
        {
            if (sources == null) throw new ArgumentNullException(nameof(sources));

            var known = new Dictionary<EnemyTypeId, EnemyTypeDefinition>();
            var prefabs = new List<KeyValuePair<EnemyTypeId, GameObject>>();

            foreach (var source in sources)
            {
                if (source == null) throw new ArgumentNullException(nameof(sources), "A source is null.");

                foreach (var type in source.ReferencedTypes)
                {
                    if (!type) continue;
                    if (!type.Id.IsValid)
                        throw new InvalidOperationException($"Enemy type '{type.name}' has no name.");

                    if (known.TryGetValue(type.Id, out var first))
                    {
                        if (first == type) continue;

                        throw new InvalidOperationException(
                            $"Two enemy types are named '{type.name}'. Names must be unique.");
                    }

                    known.Add(type.Id, type);
                    prefabs.Add(new KeyValuePair<EnemyTypeId, GameObject>(type.Id, type.Prefab));
                }
            }

            return prefabs;
        }
    }
}
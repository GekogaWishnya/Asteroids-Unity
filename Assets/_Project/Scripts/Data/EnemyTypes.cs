using System;
using System.Collections.Generic;
using Asteroids.Core.IConfigs;

namespace Asteroids.Data
{
    public static class EnemyTypes
    {
        /// <summary>Every enemy type the sources refer to.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="sources"/> or one of them is null.</exception>
        /// <exception cref="InvalidOperationException">A type has no name, or two different types share a name.</exception>
        public static IReadOnlyList<EnemyTypeDefinition> Collect(IEnumerable<IEnemyTypeSource> sources)
        {
            if (sources == null) throw new ArgumentNullException(nameof(sources));

            var known = new Dictionary<EnemyTypeId, EnemyTypeDefinition>();
            var types = new List<EnemyTypeDefinition>();

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
                    types.Add(type);
                }
            }

            return types;
        }
    }
}
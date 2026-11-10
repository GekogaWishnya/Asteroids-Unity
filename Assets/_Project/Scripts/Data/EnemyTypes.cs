using System;
using System.Collections.Generic;
using Asteroids.Core.IConfigs;

namespace Asteroids.Data
{
    public static class EnemyTypes
    {
        /// <summary>Every enemy type in play, the ones the sources refer to and every type linked from them.</summary>
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

                foreach (var type in source.ReferencedTypes) Visit(type, known, types);
            }

            return types;
        }

        private static void Visit(
            EnemyTypeDefinition type, Dictionary<EnemyTypeId, EnemyTypeDefinition> known,
            List<EnemyTypeDefinition> types)
        {
            if (!type) return;
            if (!type.Id.IsValid)
                throw new InvalidOperationException($"Enemy type '{type.name}' has no name.");

            if (known.TryGetValue(type.Id, out var first))
            {
                if (first == type) return;

                throw new InvalidOperationException($"Two enemy types are named '{type.name}'. Names must be unique.");
            }

            known.Add(type.Id, type);
            types.Add(type);

            foreach (var linked in type.LinkedTypes) Visit(linked, known, types);
        }
    }
}
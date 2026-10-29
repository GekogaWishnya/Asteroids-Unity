using System;
using System.Collections.Generic;
using Asteroids.Core;
using Asteroids.Core.IConfigs;

namespace Asteroids.Enemies
{
    /// <summary>
    /// Holds data about enemies on the level: one spawner per enemy type and total of what is alive.
    /// Does not know what kinds of enemies exist.
    /// </summary>
    public sealed class EnemyData: IEnemyData
    {
        private readonly Dictionary<EnemyTypeId, IEnemySpawner> _byType = new();
        private readonly IEnemySpawner[] _all;

        /// <exception cref="ArgumentNullException"><paramref name="spawners"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// A spawner is null or has no type, or two spawners share a type.
        /// </exception>
        public EnemyData(IEnumerable<IEnemySpawner> spawners)
        {
            if (spawners == null) throw new ArgumentNullException(nameof(spawners));

            var all = new List<IEnemySpawner>();
            foreach (var spawner in spawners)
            {
                if (spawner == null)
                    throw new ArgumentException("A spawner must not be null.", nameof(spawners));
                if (!spawner.Type.IsValid)
                    throw new ArgumentException("A spawner has no enemy type.", nameof(spawners));
                if (!_byType.TryAdd(spawner.Type, spawner))
                    throw new ArgumentException(
                        $"More than one spawner is registered for enemy type '{spawner.Type}'.", nameof(spawners));

                all.Add(spawner);
            }

            _all = all.ToArray();
        }

        public int AliveCount
        {
            get
            {
                var total = 0;
                foreach (var spawner in _all) total += spawner.AliveCount;
                return total;
            }
        }

        public bool CanSpawn(EnemyTypeId type) => type.IsValid && _byType.ContainsKey(type);

        public void Spawn(EnemyTypeId type)
        {
            if (!type.IsValid || !_byType.TryGetValue(type, out var spawner))
                throw new InvalidOperationException($"No spawner is registered for enemy type '{type}'.");

            spawner.Spawn();
        }
    }
}

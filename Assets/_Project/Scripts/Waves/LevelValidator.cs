using System;
using System.Collections.Generic;
using Asteroids.Core;
using Asteroids.Core.IConfigs;

namespace Asteroids.Waves
{
    /// <summary>
    /// Finds everything wrong with a level before it starts.
    /// </summary>
    public static class LevelValidator
    {
        /// <returns>One message per problem. Empty when the level can be played.</returns>
        /// <exception cref="ArgumentNullException">An argument is null.</exception>
        public static IReadOnlyList<string> Validate(ILevelConfig level, IEnemyData data)
        {
            if (level == null) throw new ArgumentNullException(nameof(level));
            if (data == null) throw new ArgumentNullException(nameof(data));

            var problems = new List<string>(LevelConfigValidator.Validate(level));

            var waves = level.Waves;
            if (waves == null) return problems;

            for (var i = 0; i < waves.Count; i++)
            {
                var entries = waves[i]?.Entries;
                if (entries == null) continue;

                for (var j = 0; j < entries.Count; j++)
                {
                    var entry = entries[j];
                    if (entry == null || !entry.Type.IsValid || data.CanSpawn(entry.Type)) continue;

                    problems.Add(
                        $"Wave {i + 1}, entry {j + 1}: no spawner is registered for enemy type '{entry.Type}'.");
                }
            }

            return problems;
        }

        /// <exception cref="ArgumentNullException">An argument is null.</exception>
        /// <exception cref="InvalidOperationException">The level has problems; the message lists all of them.</exception>
        public static void ThrowIfInvalid(ILevelConfig level, IEnemyData data) =>
            ConfigValidation.ThrowIfInvalid("The level", Validate(level, data));
    }
}

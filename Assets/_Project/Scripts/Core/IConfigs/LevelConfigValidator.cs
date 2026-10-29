using System;
using System.Collections.Generic;

namespace Asteroids.Core.IConfigs
{
    /// <summary>
    /// Finds mistakes in a level configuration. Waves and entries are numbered from 1.
    /// </summary>
    public static class LevelConfigValidator
    {
        /// <returns>One message per problem. Empty when the level is fine.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="level"/> is null.</exception>
        public static IReadOnlyList<string> Validate(ILevelConfig level)
        {
            if (level == null) throw new ArgumentNullException(nameof(level));

            var problems = new List<string>();
            var waves = level.Waves;
            if (waves == null || waves.Count == 0)
            {
                problems.Add("The level has no waves.");
                return problems;
            }

            for (var i = 0; i < waves.Count; i++) ValidateWave(waves[i], i + 1, problems);

            return problems;
        }

        private static void ValidateWave(IWaveConfig wave, int number, List<string> problems)
        {
            if (wave == null)
            {
                problems.Add($"Wave {number} is missing.");
                return;
            }

            if (!ConfigChecks.IsNonNegative(wave.DelayBeforeStart))
                problems.Add(
                    $"Wave {number}: delay before start must be a finite number that is not negative " +
                    $"(is {ConfigChecks.Format(wave.DelayBeforeStart)}).");

            var entries = wave.Entries;
            if (entries == null || entries.Count == 0)
            {
                problems.Add($"Wave {number} has no entries.");
                return;
            }

            for (var i = 0; i < entries.Count; i++) ValidateEntry(entries[i], number, i + 1, problems);
        }

        private static void ValidateEntry(IWaveEntryConfig entry, int wave, int number, List<string> problems)
        {
            var label = $"Wave {wave}, entry {number}";

            if (entry == null)
            {
                problems.Add($"{label} is missing.");
                return;
            }

            if (!entry.Type.IsValid)
                problems.Add($"{label}: no enemy type is set.");

            if (entry.Count < 1)
                problems.Add($"{label}: count must be at least 1 (is {entry.Count}).");

            if (!ConfigChecks.IsNonNegative(entry.StartDelay))
                problems.Add(
                    $"{label}: start delay must be a finite number that is not negative " +
                    $"(is {ConfigChecks.Format(entry.StartDelay)}).");

            if (!ConfigChecks.IsNonNegative(entry.Interval))
                problems.Add(
                    $"{label}: interval must be a finite number that is not negative " +
                    $"(is {ConfigChecks.Format(entry.Interval)}).");
        }
    }
}

using System;
using System.Collections.Generic;

namespace Asteroids.Core.IConfigs
{
    public static class SpawnEntryConfigValidator
    {
        /// <returns>One message per problem. Empty when the config is fine.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> is null.</exception>
        public static IReadOnlyList<string> Validate(ISpawnEntryConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var problems = new List<string>();

            if (!ConfigChecks.IsNonNegative(config.SpawnMargin))
                problems.Add($"Spawn margin must not be negative (is {ConfigChecks.Format(config.SpawnMargin)}).");

            if (!(float.IsFinite(config.MaxEnteringSeconds) && config.MaxEnteringSeconds > 0f))
                problems.Add(
                    $"Max entering seconds must be positive (is {ConfigChecks.Format(config.MaxEnteringSeconds)}).");

            return problems;
        }
    }
}

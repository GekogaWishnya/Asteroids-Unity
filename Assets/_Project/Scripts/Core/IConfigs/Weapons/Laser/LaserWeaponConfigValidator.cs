using System;
using System.Collections.Generic;

namespace Asteroids.Core.IConfigs
{
    public static class LaserWeaponConfigValidator
    {
        /// <returns>One message per problem. Empty when the config is fine.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> is null.</exception>
        public static IReadOnlyList<string> Validate(ILaserWeaponConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var problems = new List<string>();

            if (!IsPositive(config.MaxEnergy))
                problems.Add($"Max energy must be positive (is {ConfigChecks.Format(config.MaxEnergy)}).");

            if (!IsPositive(config.DrainPerSecond))
                problems.Add($"Drain per second must be positive (is {ConfigChecks.Format(config.DrainPerSecond)}).");

            if (!IsPositive(config.RefillPerSecond))
                problems.Add($"Refill per second must be positive (is {ConfigChecks.Format(config.RefillPerSecond)}).");

            if (!ConfigChecks.IsNonNegative(config.RefillDelay))
                problems.Add(
                    $"Refill delay must not be a negative number of seconds (is {ConfigChecks.Format(config.RefillDelay)}).");

            if (!IsPositive(config.MinEnergyToFire))
                problems.Add(
                    $"Min energy to fire must be positive (is {ConfigChecks.Format(config.MinEnergyToFire)}).");
            else if (IsPositive(config.MaxEnergy) && config.MinEnergyToFire > config.MaxEnergy)
                problems.Add(
                    $"Min energy to fire ({ConfigChecks.Format(config.MinEnergyToFire)}) must not exceed " +
                    $"max energy ({ConfigChecks.Format(config.MaxEnergy)}), or the laser can never fire.");

            if (config.HitDamage < 1)
                problems.Add($"Hit damage must be at least 1 (is {config.HitDamage}).");

            if (!IsPositive(config.HitInterval))
                problems.Add(
                    $"Hit interval must be a positive number of seconds (is {ConfigChecks.Format(config.HitInterval)}).");

            if (!ConfigChecks.IsNonNegative(config.MuzzleOffset))
                problems.Add($"Muzzle offset must not be negative (is {ConfigChecks.Format(config.MuzzleOffset)}).");

            return problems;
        }

        private static bool IsPositive(float value) => float.IsFinite(value) && value > 0f;
    }
}

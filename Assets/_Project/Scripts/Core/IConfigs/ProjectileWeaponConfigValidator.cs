using System;
using System.Collections.Generic;

namespace Asteroids.Core.IConfigs
{
    /// <summary>Finds mistakes in a projectile weapon config.</summary>
    public static class ProjectileWeaponConfigValidator
    {
        /// <returns>One message per problem. Empty when the config is fine.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> is null.</exception>
        public static IReadOnlyList<string> Validate(IProjectileWeaponConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var problems = new List<string>();

            if (!IsPositive(config.FireInterval))
                problems.Add(
                    $"Fire interval must be a positive number of seconds (is {ConfigChecks.Format(config.FireInterval)}).");

            if (!ConfigChecks.IsNonNegative(config.MuzzleOffset))
                problems.Add($"Muzzle offset must not be negative (is {ConfigChecks.Format(config.MuzzleOffset)}).");

            if (!IsPositive(config.ProjectileSpeed))
                problems.Add($"Projectile speed must be positive (is {ConfigChecks.Format(config.ProjectileSpeed)}).");

            if (!IsPositive(config.ProjectileLifetime))
                problems.Add(
                    $"Projectile lifetime must be a positive number of seconds (is {ConfigChecks.Format(config.ProjectileLifetime)}).");

            if (config.ProjectileDamage < 1)
                problems.Add($"Projectile damage must be at least 1 (is {config.ProjectileDamage}).");

            if (config.PoolPrewarm < 0)
                problems.Add($"Pool prewarm must not be negative (is {config.PoolPrewarm}).");

            if (config.PoolMaxIdle < 0)
                problems.Add($"Pool max idle must not be negative (is {config.PoolMaxIdle}).");

            return problems;
        }

        private static bool IsPositive(float value) => float.IsFinite(value) && value > 0f;
    }
}

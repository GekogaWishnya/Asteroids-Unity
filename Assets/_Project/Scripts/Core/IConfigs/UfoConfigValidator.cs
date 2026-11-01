using System;
using System.Collections.Generic;

namespace Asteroids.Core.IConfigs
{
    public static class UfoConfigValidator
    {
        /// <returns>One message per problem. Empty when the config is fine.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> is null.</exception>
        public static IReadOnlyList<string> Validate(IUfoConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var problems = new List<string>();
            var types = config.Types;
            if (types == null || types.Count == 0)
            {
                problems.Add("The UFO config has no types.");
                return problems;
            }

            var seen = new HashSet<EnemyTypeId>();
            for (var i = 0; i < types.Count; i++)
            {
                var type = types[i];
                if (type == null)
                {
                    problems.Add($"UFO type {i + 1} is missing.");
                    continue;
                }

                if (!type.Type.IsValid)
                {
                    problems.Add($"UFO type {i + 1} has no type id.");
                    continue;
                }

                if (!seen.Add(type.Type))
                {
                    problems.Add($"UFO type '{type.Type}' is defined more than once.");
                    continue;
                }

                ValidateType(type, problems);
            }

            return problems;
        }

        private static void ValidateType(IUfoTypeConfig type, List<string> problems)
        {
            var label = $"UFO type '{type.Type}'";

            if (type.MaxHealth < 1)
                problems.Add($"{label}: max health must be at least 1 (is {type.MaxHealth}).");

            if (type.ContactDamage < 1)
                problems.Add($"{label}: contact damage must be at least 1 (is {type.ContactDamage}).");

            if (type.ScorePoints < 0)
                problems.Add($"{label}: score points must not be negative (is {type.ScorePoints}).");

            var maxSpeedValid = float.IsFinite(type.MaxSpeed) && type.MaxSpeed > 0f;
            if (!maxSpeedValid)
                problems.Add(
                    $"{label}: max speed must be a finite number above 0 (is {ConfigChecks.Format(type.MaxSpeed)}).");

            RequireNonNegative(label, "launch speed", type.LaunchSpeed, problems);
            RequireNonNegative(label, "thrust", type.Thrust, problems);
            RequireNonNegative(label, "turn speed", type.TurnSpeed, problems);
            RequireNonNegative(label, "linear damping", type.LinearDamping, problems);
            RequireNonNegative(label, "min knockback speed", type.MinKnockbackSpeed, problems);
            RequireNonNegative(label, "stun seconds", type.StunSeconds, problems);

            if (maxSpeedValid && ConfigChecks.IsNonNegative(type.LaunchSpeed) && type.LaunchSpeed > type.MaxSpeed)
                problems.Add(
                    $"{label}: launch speed {ConfigChecks.Format(type.LaunchSpeed)} is above max speed " +
                    $"{ConfigChecks.Format(type.MaxSpeed)}.");

            PoolConfigRules.Validate(label, type.PoolPrewarm, type.PoolMaxIdle, problems);
        }

        private static void RequireNonNegative(string label, string what, float value, List<string> problems)
        {
            if (!ConfigChecks.IsNonNegative(value))
                problems.Add(
                    $"{label}: {what} must be a finite number that is not negative (is {ConfigChecks.Format(value)}).");
        }
    }
}

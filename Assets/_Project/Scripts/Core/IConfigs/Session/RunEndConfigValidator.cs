using System;
using System.Collections.Generic;

namespace Asteroids.Core.IConfigs
{
    public static class RunEndConfigValidator
    {
        /// <returns>One message per problem. Empty when the config is fine.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> is null.</exception>
        public static IReadOnlyList<string> Validate(IRunEndConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var problems = new List<string>();
            CheckDelay(problems, "Defeat delay", config.DefeatDelay);
            CheckDelay(problems, "Victory delay", config.VictoryDelay);
            return problems;
        }

        private static void CheckDelay(List<string> problems, string name, float seconds)
        {
            if (!ConfigChecks.IsNonNegative(seconds))
                problems.Add($"{name} must be a finite number that is not negative (is {ConfigChecks.Format(seconds)}).");
        }
    }
}

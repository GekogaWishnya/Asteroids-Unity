using System;
using System.Collections.Generic;

namespace Asteroids.Core.IConfigs
{
    public static class HealthHudConfigValidator
    {
        /// <returns>One message per problem. Empty when the config is fine.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> is null.</exception>
        public static IReadOnlyList<string> Validate(IHealthHudConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var problems = new List<string>();
            ConfigChecks.CheckFormat(problems, "Health format", config.HealthFormat, 3, 5);
            return problems;
        }
    }
}

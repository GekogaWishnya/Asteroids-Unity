using System;
using System.Collections.Generic;

namespace Asteroids.Core.IConfigs
{
    public static class LaserHudConfigValidator
    {
        /// <returns>One message per problem. Empty when the config is fine.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> is null.</exception>
        public static IReadOnlyList<string> Validate(ILaserHudConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var problems = new List<string>();
            ConfigChecks.CheckFormat(problems, "Energy format", config.EnergyFormat, 73);
            ConfigChecks.CheckFormat(problems, "Refill format", config.RefillFormat, 4.2);
            ConfigChecks.CheckText(problems, "Full text", config.FullText);
            return problems;
        }
    }
}

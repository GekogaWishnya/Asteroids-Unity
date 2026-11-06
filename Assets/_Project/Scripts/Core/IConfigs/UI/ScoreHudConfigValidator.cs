using System;
using System.Collections.Generic;

namespace Asteroids.Core.IConfigs
{
    public static class ScoreHudConfigValidator
    {
        /// <returns>One message per problem. Empty when the config is fine.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> is null.</exception>
        public static IReadOnlyList<string> Validate(IScoreHudConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var problems = new List<string>();
            ConfigChecks.CheckFormat(problems, "Score format", config.ScoreFormat, 120);
            return problems;
        }
    }
}

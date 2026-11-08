using System;
using System.Collections.Generic;

namespace Asteroids.Core.IConfigs
{
    public static class RunEndScreenConfigValidator
    {
        /// <returns>One message per problem. Empty when the config is fine.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> is null.</exception>
        public static IReadOnlyList<string> Validate(IRunEndScreenConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var problems = new List<string>();
            CheckTitle(problems, "Defeat title", config.DefeatTitle);
            CheckTitle(problems, "Victory title", config.VictoryTitle);
            ConfigChecks.CheckFormat(problems, "Score format", config.ScoreFormat, 120);
            ConfigChecks.CheckFormat(problems, "Waves format", config.WavesFormat, 2, 5);
            return problems;
        }

        private static void CheckTitle(List<string> problems, string name, string title)
        {
            if (string.IsNullOrWhiteSpace(title)) problems.Add($"{name} must not be empty.");
        }
    }
}
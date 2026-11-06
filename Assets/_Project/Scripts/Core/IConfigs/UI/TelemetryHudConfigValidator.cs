using System;
using System.Collections.Generic;

namespace Asteroids.Core.IConfigs
{
    public static class TelemetryHudConfigValidator
    {
        /// <returns>One message per problem. Empty when the config is fine.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> is null.</exception>
        public static IReadOnlyList<string> Validate(ITelemetryHudConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var problems = new List<string>();

            ConfigChecks.CheckFormat(problems, "Position format", config.PositionFormat, 1.5, -2.5);
            ConfigChecks.CheckFormat(problems, "Angle format", config.AngleFormat, 90);
            ConfigChecks.CheckFormat(problems, "Speed format", config.SpeedFormat, 3.5);
            ConfigChecks.CheckText(problems, "No-ship position text", config.NoShipPositionText);
            ConfigChecks.CheckText(problems, "No-ship angle text", config.NoShipAngleText);
            ConfigChecks.CheckText(problems, "No-ship speed text", config.NoShipSpeedText);

            return problems;
        }
    }
}

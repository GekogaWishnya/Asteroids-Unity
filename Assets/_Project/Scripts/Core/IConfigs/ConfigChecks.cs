using System.Globalization;

namespace Asteroids.Core.IConfigs
{
    internal static class ConfigChecks
    {
        public static bool IsNonNegative(float value) => float.IsFinite(value) && value >= 0f;

        public static string Format(float value) => value.ToString(CultureInfo.InvariantCulture);
    }
}

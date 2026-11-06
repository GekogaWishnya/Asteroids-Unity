using System;
using System.Collections.Generic;
using System.Globalization;

namespace Asteroids.Core.IConfigs
{
    internal static class ConfigChecks
    {
        public static bool IsNonNegative(float value) => float.IsFinite(value) && value >= 0f;

        public static string Format(float value) => value.ToString(CultureInfo.InvariantCulture);

        public static void CheckFormat(List<string> problems, string name, string format, params object[] sample)
        {
            if (string.IsNullOrEmpty(format))
            {
                problems.Add($"{name} must not be empty.");
                return;
            }

            try
            {
                _ = string.Format(CultureInfo.InvariantCulture, format, sample);
            }
            catch (FormatException)
            {
                problems.Add($"{name} '{format}' is not a valid format for {sample.Length} value(s).");
            }
        }

        public static void CheckText(List<string> problems, string name, string text)
        {
            if (text == null) problems.Add($"{name} must not be null.");
        }
    }
}
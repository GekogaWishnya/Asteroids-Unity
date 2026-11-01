using System.Collections.Generic;

namespace Asteroids.Core.IConfigs
{
    /// <summary>Rules every pooled enemy type's pool settings must follow.</summary>
    public static class PoolConfigRules
    {
        public static void Validate(string label, int prewarm, int maxIdle, List<string> problems)
        {
            if (prewarm < 0)
                problems.Add($"{label}: pool prewarm must not be negative (is {prewarm}).");

            if (maxIdle < 0)
                problems.Add($"{label}: pool max idle must not be negative (is {maxIdle}).");

            if (prewarm >= 0 && maxIdle >= 0 && prewarm > maxIdle)
                problems.Add(
                    $"{label}: pool prewarm {prewarm} is above max idle {maxIdle}; " +
                    $"the pool never keeps more than {maxIdle}, so the rest is wasted.");
        }
    }
}

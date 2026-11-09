using System;
using System.Collections.Generic;

namespace Asteroids.Core.IConfigs
{
    public static class EffectsConfigValidator
    {
        /// <returns>One message per problem. Empty when the config is fine.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> is null.</exception>
        public static IReadOnlyList<string> Validate(IEffectsConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var problems = new List<string>();

            if (config.Ship == null) problems.Add("The ship has no effects.");
            else ValidateFeedback("The ship", config.Ship, problems);

            var enemies = config.Enemies;
            if (enemies == null) return problems;

            var covered = new HashSet<EnemyTypeId>();
            for (var i = 0; i < enemies.Count; i++)
            {
                var entry = enemies[i];
                if (entry == null)
                {
                    problems.Add($"Enemy effects entry {i + 1} is missing.");
                    continue;
                }

                if (!entry.Type.IsValid)
                {
                    problems.Add($"Enemy effects entry {i + 1} has no enemy type.");
                    continue;
                }

                if (!covered.Add(entry.Type))
                {
                    problems.Add($"Enemy type '{entry.Type}' has more than one effects entry.");
                    continue;
                }

                ValidateFeedback($"Enemy type '{entry.Type}'", entry, problems);
            }

            return problems;
        }

        /// <param name="label">Names the thing at the start of each message.</param>
        /// <exception cref="ArgumentNullException"><paramref name="feedback"/> or <paramref name="problems"/> is null.</exception>
        public static void ValidateFeedback(string label, IFeedbackConfig feedback, List<string> problems)
        {
            if (feedback == null) throw new ArgumentNullException(nameof(feedback));
            if (problems == null) throw new ArgumentNullException(nameof(problems));

            if (feedback.Death != null)
                ValidateEffect($"{label}, death effect '{feedback.Death.Name}'", feedback.Death, problems);

            if (feedback.Hit != null)
                ValidateEffect($"{label}, hit effect '{feedback.Hit.Name}'", feedback.Hit, problems);
        }

        /// <param name="label">Names the effect at the start of each message.</param>
        /// <exception cref="ArgumentNullException"><paramref name="effect"/> or <paramref name="problems"/> is null.</exception>
        public static void ValidateEffect(string label, IEffectConfig effect, List<string> problems)
        {
            if (effect == null) throw new ArgumentNullException(nameof(effect));
            if (problems == null) throw new ArgumentNullException(nameof(problems));

            if (!ConfigChecks.IsPositive(effect.Scale))
                problems.Add(
                    $"{label}: scale must be a finite positive number (is {ConfigChecks.Format(effect.Scale)}).");

            if (effect.MaxActive < 1)
                problems.Add($"{label}: at least one effect must be able to play (max active is {effect.MaxActive}).");

            PoolConfigRules.Validate(label, effect.PoolPrewarm, effect.PoolMaxIdle, problems);
        }
    }
}
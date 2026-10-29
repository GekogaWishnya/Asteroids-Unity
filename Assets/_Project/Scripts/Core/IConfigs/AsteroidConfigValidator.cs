using System;
using System.Collections.Generic;

namespace Asteroids.Core.IConfigs
{
    /// <summary>
    /// Finds mistakes in an asteroid config.
    /// </summary>
    public static class AsteroidConfigValidator
    {
        /// <returns>One message per problem. Empty when the config is fine.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> is null.</exception>
        public static IReadOnlyList<string> Validate(IAsteroidConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var problems = new List<string>();
            var types = config.Types;
            if (types == null || types.Count == 0)
            {
                problems.Add("The asteroid config has no types.");
                return problems;
            }

            var indexById = new Dictionary<EnemyTypeId, int>();
            for (var i = 0; i < types.Count; i++)
            {
                var type = types[i];
                if (type == null)
                {
                    problems.Add($"Asteroid type {i + 1} is missing.");
                    continue;
                }

                if (!type.Type.IsValid)
                {
                    problems.Add($"Asteroid type {i + 1} has no type id.");
                    continue;
                }

                if (!indexById.TryAdd(type.Type, i))
                    problems.Add($"Asteroid type '{type.Type}' is defined more than once.");
            }

            foreach (var index in indexById.Values) ValidateType(types[index], indexById, problems);

            ReportSplitCycles(types, indexById, problems);

            return problems;
        }

        private static void ValidateType(
            IAsteroidTypeConfig type, Dictionary<EnemyTypeId, int> indexById, List<string> problems)
        {
            var label = $"Asteroid type '{type.Type}'";

            if (type.MaxHealth < 1)
                problems.Add($"{label}: max health must be at least 1 (is {type.MaxHealth}).");

            if (type.ContactDamage < 1)
                problems.Add($"{label}: contact damage must be at least 1 (is {type.ContactDamage}).");

            if (type.ScorePoints < 0)
                problems.Add($"{label}: score points must not be negative (is {type.ScorePoints}).");

            ValidateRange(label, "speed", type.MinSpeed, type.MaxSpeed, problems);
            ValidateRange(label, "spin speed", type.MinSpinSpeed, type.MaxSpinSpeed, problems);
            ValidateChildren(label, type, indexById, problems);
            ValidatePool(label, type, problems);
        }

        private static void ValidateRange(string label, string what, float min, float max, List<string> problems)
        {
            var minValid = ConfigChecks.IsNonNegative(min);
            var maxValid = ConfigChecks.IsNonNegative(max);

            if (!minValid)
                problems.Add(
                    $"{label}: minimum {what} must be a finite number that is not negative " +
                    $"(is {ConfigChecks.Format(min)}).");

            if (!maxValid)
                problems.Add(
                    $"{label}: maximum {what} must be a finite number that is not negative " +
                    $"(is {ConfigChecks.Format(max)}).");

            if (minValid && maxValid && max < min)
                problems.Add(
                    $"{label}: maximum {what} {ConfigChecks.Format(max)} is below the minimum " +
                    $"{ConfigChecks.Format(min)}.");
        }

        private static void ValidateChildren(
            string label, IAsteroidTypeConfig type, Dictionary<EnemyTypeId, int> indexById, List<string> problems)
        {
            var hasChildType = type.ChildType.IsValid;

            if (type.ChildCount < 0)
                problems.Add($"{label}: child count must not be negative (is {type.ChildCount}).");
            else if (hasChildType && type.ChildCount == 0)
                problems.Add($"{label}: child type '{type.ChildType}' is set but the child count is 0.");
            else if (!hasChildType && type.ChildCount > 0)
                problems.Add($"{label}: child count is {type.ChildCount} but no child type is set.");

            if (hasChildType && !indexById.ContainsKey(type.ChildType))
                problems.Add($"{label}: child type '{type.ChildType}' is not defined in this asteroid config.");
        }

        private static void ValidatePool(string label, IAsteroidTypeConfig type, List<string> problems)
        {
            if (type.PoolPrewarm < 0)
                problems.Add($"{label}: pool prewarm must not be negative (is {type.PoolPrewarm}).");

            if (type.PoolMaxIdle < 0)
                problems.Add($"{label}: pool max idle must not be negative (is {type.PoolMaxIdle}).");

            if (type.PoolPrewarm >= 0 && type.PoolMaxIdle >= 0 && type.PoolPrewarm > type.PoolMaxIdle)
                problems.Add(
                    $"{label}: pool prewarm {type.PoolPrewarm} is above max idle {type.PoolMaxIdle}; " +
                    $"the pool never keeps more than {type.PoolMaxIdle}, so the rest is wasted.");
        }

        /// <summary>
        /// Every type has at most one child type, so a cycle is found by following the chain from each type.
        /// A cycle is reported once, from its member that comes first in the config.
        /// </summary>
        private static void ReportSplitCycles(
            IReadOnlyList<IAsteroidTypeConfig> types, Dictionary<EnemyTypeId, int> indexById, List<string> problems)
        {
            foreach (var start in indexById.Values)
            {
                var visited = new HashSet<int> { start };
                var chain = new List<string> { types[start].Type.ToString() };
                var current = start;

                while (TryGetChild(types, indexById, current, out var next))
                {
                    chain.Add(types[next].Type.ToString());

                    if (next == start)
                    {
                        if (IsFirstInConfig(visited, start))
                            problems.Add($"Split cycle: {string.Join(" -> ", chain)}.");
                        break;
                    }

                    // The chain ran into a cycle that does not contain the start; its members report it.
                    if (!visited.Add(next)) break;

                    current = next;
                }
            }
        }

        private static bool TryGetChild(
            IReadOnlyList<IAsteroidTypeConfig> types, Dictionary<EnemyTypeId, int> indexById, int index, out int child)
        {
            var type = types[index];
            child = -1;

            return type.ChildCount > 0
                   && type.ChildType.IsValid
                   && indexById.TryGetValue(type.ChildType, out child);
        }

        private static bool IsFirstInConfig(HashSet<int> members, int candidate)
        {
            foreach (var member in members)
                if (member < candidate) return false;

            return true;
        }
    }
}

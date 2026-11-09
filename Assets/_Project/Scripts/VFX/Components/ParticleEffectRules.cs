using System;
using System.Collections.Generic;
using UnityEngine;

namespace Asteroids.VFX.Components
{
    /// <summary>Assures that a pooled particle effect follows the intended rules.</summary>
    public static class ParticleEffectRules
    {
        /// <returns>One message per problem. Empty when the effect can be pooled.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="root"/> is null.</exception>
        public static IReadOnlyList<string> FindProblems(ParticleSystem root)
        {
            if (!root) throw new ArgumentNullException(nameof(root));

            var problems = new List<string>();

            if (root.main.stopAction != ParticleSystemStopAction.Callback)
                problems.Add($"'{root.name}': Stop Action must be Callback (is {root.main.stopAction}).");

            foreach (var system in root.GetComponentsInChildren<ParticleSystem>(true))
                CheckSystem(system, problems);

            return problems;
        }

        private static void CheckSystem(ParticleSystem system, List<string> problems)
        {
            var label = $"'{system.name}'";
            var main = system.main;

            if (main.loop) problems.Add($"{label}: Looping must be off.");
            if (main.playOnAwake) problems.Add($"{label}: Play On Awake must be off.");
            if (main.scalingMode != ParticleSystemScalingMode.Hierarchy)
                problems.Add($"{label}: Scaling Mode must be Hierarchy (is {main.scalingMode}).");
            if (system.collision.enabled)
                problems.Add($"{label}: the Collision module must be off (no Unity physics).");
            if (system.trigger.enabled) problems.Add($"{label}: the Triggers module must be off (no Unity physics).");
            if (system.externalForces.enabled)
                problems.Add($"{label}: the External Forces module must be off (no Unity physics).");
        }
    }
}
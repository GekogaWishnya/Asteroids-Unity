using System.Collections.Generic;
using Asteroids.Data;
using UnityEngine;

namespace Asteroids.VFX.Components
{
    [CreateAssetMenu(fileName = "ParticleEffect", menuName = "Asteroids/Effects/Particle Effect")]
    public sealed class ParticleEffectDefinition : EffectDefinition
    {
        [Tooltip("What the effect is made of. " +
                 "Its root needs a Particle Effect component next to the root particle system")]
        [SerializeField] private ParticleEffect prefab;

        public ParticleEffect Prefab => prefab;

        /// <returns>Everything wrong with the settings and the prefab. Empty when the effect can be played.</returns>
        public override IReadOnlyList<string> FindProblems()
        {
            var problems = new List<string>(base.FindProblems());

            if (!prefab) problems.Add("has no prefab.");
            else problems.AddRange(ParticleEffectRules.FindProblems(prefab.Particles));

            return problems;
        }
    }
}
using System;
using Asteroids.Core.IConfigs;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Asteroids.VFX.Components
{
    public sealed class PrefabEffectFactory : IEffectFactory
    {
        private Transform _idleRoot;
        private Transform _activeRoot;

        /// <exception cref="ArgumentNullException"><paramref name="config"/> is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// The config is not a <see cref="ParticleEffectDefinition"/>, or its prefab is missing or breaks the rules.
        /// </exception>
        public IEffect Create(IEffectConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (config is not ParticleEffectDefinition definition || !definition)
                throw new InvalidOperationException(
                    $"Effect '{config.Name}' is not a {nameof(ParticleEffectDefinition)}, so it has no prefab.");

            var problems = definition.FindProblems();
            ConfigValidation.ThrowIfInvalid($"Particle effect '{definition.name}'", problems);

            EnsureRoots();

            var instance = Object.Instantiate(definition.Prefab, _idleRoot);
            instance.name = definition.Prefab.name;
            return new PooledParticleEffect(instance, _idleRoot, _activeRoot);
        }

        private void EnsureRoots()
        {
            if (_idleRoot && _activeRoot) return;

            var idle = new GameObject("Effect pool (idle)");
            idle.SetActive(false);
            _idleRoot = idle.transform;

            _activeRoot = new GameObject("Effects").transform;
        }
    }
}
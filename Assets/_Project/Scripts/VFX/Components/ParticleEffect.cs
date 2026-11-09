using System;
using UnityEngine;

namespace Asteroids.VFX.Components
{
    /// <summary>
    /// Root of a pooled one-shot particle effect. In the prefab, it sits next to the root <see cref="ParticleSystem"/>
    /// and forwards Unity's stop callback.
    /// </summary>
    [AddComponentMenu("Asteroids/Effects/Particle Effect")]
    [RequireComponent(typeof(ParticleSystem))]
    [DisallowMultipleComponent]
    public sealed class ParticleEffect : MonoBehaviour
    {
        private ParticleSystem _system;

        /// <summary>
        /// Raised when the root system and all its sub-systems have stopped and have no particles left.
        /// </summary>
        public event Action Stopped;

        public ParticleSystem Particles => _system ? _system : _system = GetComponent<ParticleSystem>();

        private void OnParticleSystemStopped() => Stopped?.Invoke();

        private void OnDestroy() => Stopped = null;

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall -= ValidateWhenSettled;
            UnityEditor.EditorApplication.delayCall += ValidateWhenSettled;
        }

        private void ValidateWhenSettled()
        {
            if (!this) return;

            if (UnityEditor.EditorApplication.isUpdating)
            {
                OnValidate();
                return;
            }

            foreach (var problem in ParticleEffectRules.FindProblems(Particles))
                Debug.LogWarning($"Particle effect '{name}': {problem}", this);
        }
#endif
    }
}
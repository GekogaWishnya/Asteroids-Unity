using System.Collections.Generic;
using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Data
{
    public abstract class EffectDefinition : ScriptableObject, IEffectConfig
    {
        [Tooltip("Uniform scale the effect is played at, on top of the prefab's own scale")]
        [SerializeField, Min(0.01f)] private float scale = 1f;

        [Header("Pool")]
        [Tooltip("Objects created ahead of time")]
        [SerializeField, Min(0)] private int poolPrewarm = 4;
        [Tooltip("Most finished objects kept for reuse")]
        [SerializeField, Min(0)] private int poolMaxIdle = 8;
        [Tooltip("Most objects playing at once. Playing one more recycles the oldest")]
        [SerializeField, Min(1)] private int maxActive = 16;

        public string Name => name;
        public float Scale => scale;
        public int PoolPrewarm => poolPrewarm;
        public int PoolMaxIdle => poolMaxIdle;
        public int MaxActive => maxActive;

        /// <returns>Everything wrong with the effect. Empty when it can be played.</returns>
        public virtual IReadOnlyList<string> FindProblems()
        {
            var problems = new List<string>();
            EffectsConfigValidator.ValidateEffect("Settings", this, problems);
            return problems;
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
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

            foreach (var problem in FindProblems())
                Debug.LogWarning($"Effect '{name}': {problem}", this);
        }
#endif
    }
}
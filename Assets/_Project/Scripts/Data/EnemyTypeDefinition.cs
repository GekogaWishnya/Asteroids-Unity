using System;
using System.Collections.Generic;
using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Data
{
    /// <summary>
    /// One kind of enemy as a designer picks it.
    /// The game identifies the type by the asset's name, so names must be unique.
    /// </summary>
    public abstract class EnemyTypeDefinition : ScriptableObject, IEnemyFeedbackConfig
    {
        [Header("Identity")]
        [Tooltip("What the enemy is made of. The root needs an EntityComponent")]
        [SerializeField] private GameObject prefab;

        [Tooltip("Effects played when an enemy of this type is hit and destroyed")]
        [SerializeField] private FeedbackData effects = new();

        /// <summary>The asset name. <c>default</c> (not valid) for an unnamed instance.</summary>
        public EnemyTypeId Id => string.IsNullOrWhiteSpace(name) ? default : new EnemyTypeId(name);

        public GameObject Prefab => prefab;

        /// <summary>Other types this one references (asteroids splitting).</summary>
        public virtual IEnumerable<EnemyTypeDefinition> LinkedTypes => Array.Empty<EnemyTypeDefinition>();

        EnemyTypeId IEnemyFeedbackConfig.Type => Id;
        IEffectConfig IFeedbackConfig.Hit => effects.Hit;
        bool IFeedbackConfig.PlayHitOnFatal => effects.PlayHitOnFatal;
        IEffectConfig IFeedbackConfig.Death => effects.Death;

        protected abstract void FindKindProblems(List<string> problems);

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

            var problems = new List<string>();
            if (!prefab) problems.Add("has no prefab.");
            EffectsConfigValidator.ValidateFeedback("Effects", effects, problems);
            FindKindProblems(problems);

            foreach (var problem in problems)
                Debug.LogWarning($"Enemy type '{name}': {problem}", this);
        }
#endif
    }
}
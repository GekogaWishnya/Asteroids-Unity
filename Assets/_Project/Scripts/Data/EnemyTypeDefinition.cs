using System.Collections.Generic;
using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Data
{
    /// <summary>
    /// One kind of enemy as a designer picks it.
    /// The game identifies the type by the asset's name, so names must be unique.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyType", menuName = "Asteroids/Enemies/Enemy Type")]
    public sealed class EnemyTypeDefinition : ScriptableObject, IEnemyFeedbackConfig
    {
        [Tooltip("What the enemy is made of. The root needs an EntityComponent")]
        [SerializeField] private GameObject prefab;

        [Tooltip("Effects played when an enemy of this type is hit and destroyed")]
        [SerializeField] private FeedbackData effects = new();

        /// <summary>The asset name. <c>default</c> (not valid) for an unnamed instance.</summary>
        public EnemyTypeId Id => string.IsNullOrWhiteSpace(name) ? default : new EnemyTypeId(name);

        public GameObject Prefab => prefab;

        EnemyTypeId IEnemyFeedbackConfig.Type => Id;
        IEffectConfig IFeedbackConfig.Hit => effects.Hit;
        bool IFeedbackConfig.PlayHitOnFatal => effects.PlayHitOnFatal;
        IEffectConfig IFeedbackConfig.Death => effects.Death;

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

            var problems = new List<string>();
            if (!prefab) problems.Add("has no prefab.");
            EffectsConfigValidator.ValidateFeedback("Effects", effects, problems);

            foreach (var problem in problems)
                Debug.LogWarning($"Enemy type '{name}': {problem}", this);
        }
#endif
    }
}
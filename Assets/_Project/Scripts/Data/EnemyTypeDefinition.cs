using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Data
{
    /// <summary>
    /// One kind of enemy as a designer picks it. Levels and enemy configs refer to the asset;
    /// the game identifies the type by the asset's name, so names must be unique.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyType", menuName = "Asteroids/Enemies/Enemy Type")]
    public sealed class EnemyTypeDefinition : ScriptableObject
    {
        [Tooltip("What the enemy is made of. The root needs an EntityComponent")]
        [SerializeField] private GameObject prefab;

        /// <summary>The asset name. <c>default</c> (not valid) for an unnamed instance.</summary>
        public EnemyTypeId Id => string.IsNullOrWhiteSpace(name) ? default : new EnemyTypeId(name);

        public GameObject Prefab => prefab;

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

            if (!prefab)
                Debug.LogWarning($"Enemy type '{name}' has no prefab.", this);
        }
#endif
    }
}

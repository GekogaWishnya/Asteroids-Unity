using System.Collections.Generic;
using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Data
{
    [CreateAssetMenu(fileName = "Asteroid", menuName = "Asteroids/Enemies/Asteroid Type")]
    public sealed class AsteroidTypeDefinition : EnemyTypeDefinition, IAsteroidTypeConfig
    {
        [Header("Combat")]
        [Tooltip("Hits it takes to destroy it")]
        [SerializeField, Min(1)] private int maxHealth = 1;
        [Tooltip("Damage dealt to whatever it touches and may hurt")]
        [SerializeField, Min(1)] private int contactDamage = 1;
        [Tooltip("Points awarded when it is destroyed")]
        [SerializeField, Min(0)] private int scorePoints;

        [Header("Motion")]
        [Tooltip("Units per second, rolled on spawn")]
        [SerializeField, Min(0f)] private float minSpeed = 1f;
        [Tooltip("Units per second, rolled on spawn")]
        [SerializeField, Min(0f)] private float maxSpeed = 2f;
        [Tooltip("Degrees per second, rolled on spawn. The direction is random")]
        [SerializeField, Min(0f)] private float minSpinSpeed = 20f;
        [Tooltip("Degrees per second, rolled on spawn. The direction is random")]
        [SerializeField, Min(0f)] private float maxSpinSpeed = 60f;

        [Header("Splitting")]
        [Tooltip("Asteroid type it splits into when destroyed. Empty when it does not split")]
        [SerializeField] private AsteroidTypeDefinition childType;
        [SerializeField, Min(0)] private int childCount;

        [Header("Pool")]
        [Tooltip("Instances created ahead of time")]
        [SerializeField, Min(0)] private int poolPrewarm = 8;
        [Tooltip("Most released instances kept. The pool still grows beyond it on demand")]
        [SerializeField, Min(0)] private int poolMaxIdle = 16;

        public override IEnumerable<EnemyTypeDefinition> LinkedTypes
        {
            get
            {
                if (childType) yield return childType;
            }
        }

        EnemyTypeId IAsteroidTypeConfig.Type => Id;
        public int MaxHealth => maxHealth;
        public int ContactDamage => contactDamage;
        public int ScorePoints => scorePoints;
        public float MinSpeed => minSpeed;
        public float MaxSpeed => maxSpeed;
        public float MinSpinSpeed => minSpinSpeed;
        public float MaxSpinSpeed => maxSpinSpeed;
        public EnemyTypeId ChildType => childType ? childType.Id : default;
        public int ChildCount => childCount;
        public int PoolPrewarm => poolPrewarm;
        public int PoolMaxIdle => poolMaxIdle;

        protected override void FindKindProblems(List<string> problems) =>
            AsteroidConfigValidator.ValidateType("Asteroid settings", this, problems);
    }
}
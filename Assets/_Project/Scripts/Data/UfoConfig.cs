using System;
using System.Collections.Generic;
using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Data
{
    [CreateAssetMenu(fileName = "Ufo", menuName = "Asteroids/Configs/Ufo")]
    public sealed class UfoConfig : ScriptableObject, IUfoConfig
    {
        [SerializeField] private List<TypeData> types = new();

        public IReadOnlyList<IUfoTypeConfig> Types => types;

        private void OnValidate()
        {
            foreach (var problem in UfoConfigValidator.Validate(this))
                Debug.LogWarning($"{name}: {problem}", this);

            foreach (var type in types)
            {
                if (type == null || type.LinearDamping <= 0f) continue;

                var reachable = type.Thrust / type.LinearDamping;
                if (reachable < type.MaxSpeed)
                    Debug.LogWarning(
                        $"{name}: UFO type '{type.Type}' can't reach its max speed {type.MaxSpeed} under thrust; " +
                        $"thrust / damping caps it at {reachable}.", this);
            }
        }

        [Serializable]
        private sealed class TypeData : IUfoTypeConfig
        {
            [Header("Identity")]
            [SerializeField] private EnemyTypeDefinition type;

            [Header("Combat")]
            [Tooltip("Hits it takes to destroy it. The ship's ram deals its ram damage too")]
            [SerializeField, Min(1)] private int maxHealth = 1;
            [Tooltip("Damage dealt to whatever it touches and may hurt")]
            [SerializeField, Min(1)] private int contactDamage = 1;
            [Tooltip("Points awarded when it is destroyed")]
            [SerializeField, Min(0)] private int scorePoints = 150;

            [Header("Homing")]
            [Tooltip("Speed it enters the play area with (units/s). At most max speed")]
            [SerializeField, Min(0f)] private float launchSpeed = 2f;
            [Tooltip("Acceleration along its nose (units/s²)")]
            [SerializeField, Min(0f)] private float thrust = 6f;
            [Tooltip("Fastest it turns toward the ship (degrees/s). Below the ship's turn speed, so the ship can out-turn it")]
            [SerializeField, Min(0f)] private float turnSpeed = 120f;
            [Tooltip("Speed cap (units/s)")]
            [SerializeField, Min(0.1f)] private float maxSpeed = 5f;
            [Tooltip("Slow decay of velocity. 0 is pure inertia")]
            [SerializeField, Min(0f)] private float linearDamping = 0.5f;

            [Header("Collision response")]
            [Tooltip("Slowest speed it is knocked away with (units/s), even if it stood still")]
            [SerializeField, Min(0f)] private float minKnockbackSpeed = 3f;
            [Tooltip("Seconds it stops steering and thrusting after a knockback, so it drifts away")]
            [SerializeField, Min(0f)] private float stunSeconds = 1.5f;

            [Header("Pool")]
            [Tooltip("Instances created ahead of time")]
            [SerializeField, Min(0)] private int poolPrewarm = 4;
            [Tooltip("Most released instances kept. The pool still grows beyond it on demand")]
            [SerializeField, Min(0)] private int poolMaxIdle = 8;

            public EnemyTypeId Type => type ? type.Id : default;
            public int MaxHealth => maxHealth;
            public int ContactDamage => contactDamage;
            public int ScorePoints => scorePoints;
            public float LaunchSpeed => launchSpeed;
            public float Thrust => thrust;
            public float TurnSpeed => turnSpeed;
            public float MaxSpeed => maxSpeed;
            public float LinearDamping => linearDamping;
            public float MinKnockbackSpeed => minKnockbackSpeed;
            public float StunSeconds => stunSeconds;
            public int PoolPrewarm => poolPrewarm;
            public int PoolMaxIdle => poolMaxIdle;
        }
    }
}

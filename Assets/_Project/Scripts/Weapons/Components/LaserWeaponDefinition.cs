using System;
using System.Collections.Generic;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Weapons.Components
{
    /// <summary>
    /// <para>
    /// A laser whose beam is made from a prefab. The prefab's root needs an EntityComponent and a
    /// collider, normally a Box Collider on the PlayerProjectile layer with the Enemy mask.
    /// The beam does not wrap around the world.
    /// </para>
    /// <para>
    /// An Animator on the prefab restarts from its default state every time the beam comes on and stops when it
    /// goes out.
    /// </para>
    /// </summary>
    [CreateAssetMenu(fileName = "LaserWeapon", menuName = "Asteroids/Weapons/Laser Weapon")]
    public sealed class LaserWeaponDefinition : WeaponDefinition, ILaserWeaponConfig
    {
        [Header("Beam")]
        [Tooltip("What the beam is made of. The root needs an EntityComponent and a collider")]
        [SerializeField]
        private GameObject beamPrefab;

        [Tooltip("Distance from the carrier's position where the beam starts")] [SerializeField, Min(0f)]
        private float muzzleOffset = 0.5f;

        [Header("Energy")] [Tooltip("Energy of a full tank. The tank starts empty")] [SerializeField, Min(0.01f)]
        private float maxEnergy = 100f;

        [Tooltip("Energy the beam uses per second")] [SerializeField, Min(0.01f)]
        private float drainPerSecond = 50f;

        [Tooltip("Energy regained per second while refilling. The beam never refills while firing")]
        [SerializeField, Min(0.01f)]
        private float refillPerSecond = 5f;

        [Tooltip("Seconds after the beam stops before the tank starts refilling")] [SerializeField, Min(0f)]
        private float refillDelay = 1f;

        [Tooltip("Energy a press needs to start the beam")] [SerializeField, Min(0.01f)]
        private float minEnergyToFire = 20f;

        [Header("Damage")] [Tooltip("Damage of one hit")] [SerializeField, Min(1)]
        private int hitDamage = 1;

        [Tooltip("Seconds between two hits on a target that stays in the beam. The first hit lands on contact")]
        [SerializeField, Min(0.01f)]
        private float hitInterval = 0.1f;

        public float MaxEnergy => maxEnergy;
        public float DrainPerSecond => drainPerSecond;
        public float RefillPerSecond => refillPerSecond;
        public float RefillDelay => refillDelay;
        public float MinEnergyToFire => minEnergyToFire;
        public int HitDamage => hitDamage;
        public float HitInterval => hitInterval;
        public float MuzzleOffset => muzzleOffset;

        /// <summary>The laser shows its energy on the HUD.</summary>
        public override IReadOnlyList<Type> StatusContracts { get; } = new[] { typeof(ILaserStatus) };

        public GameObject BeamPrefab => beamPrefab;

        public override IReadOnlyList<string> FindProblems()
        {
            var problems = new List<string>(LaserWeaponConfigValidator.Validate(this));
            if (!beamPrefab) problems.Add("has no beam prefab.");
            return problems;
        }

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

            foreach (var problem in FindProblems())
                Debug.LogWarning($"{name}: {problem}", this);
        }
#endif
    }
}
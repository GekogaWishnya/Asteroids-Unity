using System;
using System.Collections.Generic;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Weapons.Projectiles;
using UnityEngine;
using Zenject;

namespace Asteroids.Weapons.Components
{
    /// <summary>
    /// A gun that fires projectiles made from <see cref="ProjectilePrefab"/>. The prefab's root needs an
    /// EntityComponent and a collider. The numbers are read live, so they can be tuned while playing.
    /// The prefab is read whenever a new projectile object is made.
    /// </summary>
    [CreateAssetMenu(fileName = "ProjectileWeapon", menuName = "Asteroids/Weapons/Projectile Weapon")]
    public sealed class ProjectileWeaponDefinition : WeaponDefinition, IProjectileWeaponConfig
    {
        [Header("Projectile")]
        [Tooltip("What a projectile is made of. The root needs an EntityComponent and a collider on the Projectile layer")]
        [SerializeField] private GameObject projectilePrefab;
        [Tooltip("Units per second")]
        [SerializeField, Min(0.01f)] private float projectileSpeed = 12f;
        [Tooltip("Seconds a projectile flies before it expires")]
        [SerializeField, Min(0.01f)] private float projectileLifetime = 1f;
        [Tooltip("Damage dealt to whatever it hits and may hurt")]
        [SerializeField, Min(1)] private int projectileDamage = 1;

        [Header("Firing")]
        [Tooltip("Seconds between two shots while the trigger is held")]
        [SerializeField, Min(0.01f)] private float fireInterval = 0.25f;
        [Tooltip("Distance from the carrier's position, along its facing, where projectiles appear")]
        [SerializeField, Min(0f)] private float muzzleOffset = 0.5f;

        [Header("Pool")]
        [Tooltip("Projectiles created ahead of time")]
        [SerializeField, Min(0)] private int poolPrewarm = 8;
        [Tooltip("Most released projectiles kept. The pool still grows beyond it on demand")]
        [SerializeField, Min(0)] private int poolMaxIdle = 16;

        public GameObject ProjectilePrefab => projectilePrefab;

        public float FireInterval => fireInterval;
        public float MuzzleOffset => muzzleOffset;
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileLifetime => projectileLifetime;
        public int ProjectileDamage => projectileDamage;
        public int PoolPrewarm => poolPrewarm;
        public int PoolMaxIdle => poolMaxIdle;

        public override IWeapon Create(DiContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            ConfigValidation.ThrowIfInvalid($"Projectile weapon '{name}'", FindProblems());

            var loop = container.Resolve<ISimulationLoop>();
            var views = new PrefabProjectileViewFactory(container, this);
            var pool = new ProjectilePool(
                this, views, loop, container.Resolve<IDamageRegistry>(), container.Resolve<IProjectileRegistry>());

            pool.Prewarm();
            return new ProjectileWeapon(this, loop, pool);
        }

        private IReadOnlyList<string> FindProblems()
        {
            var problems = new List<string>(ProjectileWeaponConfigValidator.Validate(this));
            if (projectilePrefab == null) problems.Add("It has no projectile prefab.");
            return problems;
        }

        private void OnValidate()
        {
            foreach (var problem in FindProblems())
                Debug.LogWarning($"{name}: {problem}", this);
        }
    }
}

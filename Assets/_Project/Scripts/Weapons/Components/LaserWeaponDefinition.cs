using System;
using System.Collections.Generic;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Damage;
using Asteroids.Weapons.Laser;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

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
        [SerializeField] private GameObject beamPrefab;
        [Tooltip("Distance from the carrier's position where the beam starts")]
        [SerializeField, Min(0f)] private float muzzleOffset = 0.5f;

        [Header("Energy")]
        [Tooltip("Energy of a full tank. The tank starts empty")]
        [SerializeField, Min(0.01f)] private float maxEnergy = 100f;
        [Tooltip("Energy the beam uses per second")]
        [SerializeField, Min(0.01f)] private float drainPerSecond = 50f;
        [Tooltip("Energy regained per second while refilling. The beam never refills while firing")]
        [SerializeField, Min(0.01f)] private float refillPerSecond = 5f;
        [Tooltip("Seconds after the beam stops before the tank starts refilling")]
        [SerializeField, Min(0f)] private float refillDelay = 1f;
        [Tooltip("Energy a press needs to start the beam")]
        [SerializeField, Min(0.01f)] private float minEnergyToFire = 20f;

        [Header("Damage")]
        [Tooltip("Damage of one hit")]
        [SerializeField, Min(1)] private int hitDamage = 1;
        [Tooltip("Seconds between two hits on a target that stays in the beam. The first hit lands on contact")]
        [SerializeField, Min(0.01f)] private float hitInterval = 0.1f;

        public float MaxEnergy => maxEnergy;
        public float DrainPerSecond => drainPerSecond;
        public float RefillPerSecond => refillPerSecond;
        public float RefillDelay => refillDelay;
        public float MinEnergyToFire => minEnergyToFire;
        public int HitDamage => hitDamage;
        public float HitInterval => hitInterval;
        public float MuzzleOffset => muzzleOffset;

        public override IWeapon Create(DiContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            ConfigValidation.ThrowIfInvalid($"Laser weapon '{name}'", FindProblems());

            return new LaserWeapon(
                this,
                container.Resolve<ISimulationLoop>(),
                CreateBeam(container),
                container.Resolve<ILaserBeamRegistry>(),
                container.Resolve<IDamageLookup>());
        }

        /// <summary>Binds the laser as <see cref="ILaserStatus"/> for the HUD.</summary>
        /// <exception cref="InvalidOperationException">The container already has a laser status.</exception>
        public override void BindStatus(DiContainer container, Func<DiContainer, IWeapon> weapon)
        {
            base.BindStatus(container, weapon);

            if (container.HasBinding<ILaserStatus>())
                throw new InvalidOperationException(
                    $"Laser weapon '{name}' cannot be bound: a loadout can carry one laser at most.");

            container.Bind<ILaserStatus>()
                .FromMethod(context => weapon(context.Container) as ILaserStatus
                                       ?? throw new InvalidOperationException(
                                           $"Laser weapon '{name}' built a weapon that has no laser status."))
                .AsSingle();
        }

        /// <exception cref="InvalidOperationException">The prefab has no component implementing <see cref="IEntityProvider"/>.</exception>
        private ILaserBeamView CreateBeam(DiContainer container)
        {
            var idle = new GameObject($"Laser beam of {name} (idle)");
            idle.SetActive(false);
            var active = new GameObject($"Laser beam of {name}");

            var instance = container.InstantiatePrefab(beamPrefab, idle.transform);
            if (instance.TryGetComponent<IEntityProvider>(out var provider))
                return new LaserBeamView(instance.transform, provider.Entity, idle.transform, active.transform);

            Object.Destroy(instance);
            Object.Destroy(idle);
            Object.Destroy(active);
            throw new InvalidOperationException(
                $"The beam prefab '{beamPrefab.name}' of laser weapon '{name}' needs a component " +
                $"implementing {nameof(IEntityProvider)} (add EntityComponent).");
        }

        private IReadOnlyList<string> FindProblems()
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

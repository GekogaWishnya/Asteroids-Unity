using System;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Damage;
using Asteroids.Weapons.Laser;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Asteroids.Weapons.Components
{
    public sealed class LaserWeaponFactory : WeaponFactory<LaserWeaponDefinition>
    {
        private readonly ISimulationLoop _loop;
        private readonly ILaserBeamRegistry _beams;
        private readonly IDamageLookup _damage;
        private readonly IInstantiator _instantiator;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public LaserWeaponFactory(
            ISimulationLoop loop, ILaserBeamRegistry beams, IDamageLookup damage, IInstantiator instantiator)
        {
            _loop = loop ?? throw new ArgumentNullException(nameof(loop));
            _beams = beams ?? throw new ArgumentNullException(nameof(beams));
            _damage = damage ?? throw new ArgumentNullException(nameof(damage));
            _instantiator = instantiator ?? throw new ArgumentNullException(nameof(instantiator));
        }

        protected override IWeapon Create(LaserWeaponDefinition definition)
        {
            ConfigValidation.ThrowIfInvalid($"Laser weapon '{definition.name}'", definition.FindProblems());

            return new LaserWeapon(definition, _loop, CreateBeam(definition), _beams, _damage);
        }

        /// <exception cref="InvalidOperationException">The prefab has no component implementing <see cref="IEntityProvider"/>.</exception>
        private ILaserBeamView CreateBeam(LaserWeaponDefinition definition)
        {
            var idle = new GameObject($"Laser beam of {definition.name} (idle)");
            idle.SetActive(false);
            var active = new GameObject($"Laser beam of {definition.name}");

            var prefab = definition.BeamPrefab;
            var instance = _instantiator.InstantiatePrefab(prefab, idle.transform);
            if (instance.TryGetComponent<IEntityProvider>(out var provider))
                return new LaserBeamView(instance.transform, provider.Entity, idle.transform, active.transform);

            Object.Destroy(instance);
            Object.Destroy(idle);
            Object.Destroy(active);
            throw new InvalidOperationException(
                $"The beam prefab '{prefab.name}' of laser weapon '{definition.name}' needs a component " +
                $"implementing {nameof(IEntityProvider)} (add EntityComponent).");
        }
    }
}
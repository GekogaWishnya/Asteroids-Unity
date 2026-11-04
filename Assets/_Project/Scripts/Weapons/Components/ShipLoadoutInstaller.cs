using System;
using Asteroids.Core;
using Zenject;

namespace Asteroids.Weapons.Components
{
    /// <summary>
    /// Binds a factory per weapon kind (<see cref="WeaponFactories"/>) and <see cref="IShipLoadout"/> built from
    /// the given loadout when first resolved. Binds each status contract the loadout's weapons declare (such as
    /// <see cref="ILaserStatus"/>) to the weapon of its slot.
    /// Requires <see cref="WeaponsInstaller"/>, and the simulation, world and damage bindings the factories need.
    /// </summary>
    public sealed class ShipLoadoutInstaller : Installer<ShipLoadoutDefinition, ShipLoadoutInstaller>
    {
        private readonly ShipLoadoutDefinition _loadout;

        /// <param name="loadout">Null binds an empty loadout.</param>
        public ShipLoadoutInstaller(ShipLoadoutDefinition loadout) => _loadout = loadout;

        /// <exception cref="InvalidOperationException">Two weapons of the loadout provide the same status.</exception>
        public override void InstallBindings()
        {
            Container.Bind<IWeaponFactory>().To<ProjectileWeaponFactory>().AsSingle();
            Container.Bind<IWeaponFactory>().To<LaserWeaponFactory>().AsSingle();

            // Zenject injects List, so the list is resolved explicitly.
            Container.Bind<WeaponFactories>()
                .FromMethod(context => new WeaponFactories(context.Container.ResolveAll<IWeaponFactory>()))
                .AsSingle();

            var primary = _loadout ? _loadout.Primary : null;
            var alternate = _loadout ? _loadout.Alternate : null;
            if (!primary) primary = null;
            if (!alternate) alternate = null;

            Container.Bind<IShipLoadout>()
                .FromMethod(context =>
                {
                    var factories = context.Container.Resolve<WeaponFactories>();
                    return new WeaponLoadout(
                        primary ? factories.Create(primary) : null,
                        alternate ? factories.Create(alternate) : null);
                })
                .AsSingle();

            foreach (var (contract, slot) in WeaponStatusContracts.Collect(primary, alternate))
            {
                if (Container.HasBinding(contract))
                    throw new InvalidOperationException(
                        $"{contract.Name} is already bound, so the {slot} weapon of the loadout can't provide it.");

                Container.Bind(contract)
                    .FromMethodUntyped(context => WeaponStatusContracts.Resolve(
                        context.Container.Resolve<IShipLoadout>(), slot, contract))
                    .AsSingle();
            }
        }
    }
}
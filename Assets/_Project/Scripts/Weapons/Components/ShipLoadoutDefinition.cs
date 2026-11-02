using System;
using Asteroids.Core;
using UnityEngine;
using Zenject;

namespace Asteroids.Weapons.Components
{
    /// <summary>Weapons the ship carries in a level, one per slot. Both slots are optional.</summary>
    [CreateAssetMenu(fileName = "ShipLoadout", menuName = "Asteroids/Weapons/Ship Loadout")]
    public sealed class ShipLoadoutDefinition : ScriptableObject
    {
        [Tooltip("Fired with Fire. Leave empty for none")]
        [SerializeField] private WeaponDefinition primary;
        [Tooltip("Fired with Alt Fire. Leave empty for none")]
        [SerializeField] private WeaponDefinition alternate;

        public WeaponDefinition Primary => primary;
        public WeaponDefinition Alternate => alternate;

        /// <summary>Binds <see cref="IShipLoadout"/>, built from <paramref name="loadout"/>.</summary>
        /// <param name="loadout">Null binds an empty loadout.</param>
        /// <exception cref="ArgumentNullException"><paramref name="container"/> is null.</exception>
        public static void Install(DiContainer container, ShipLoadoutDefinition loadout)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            var primary = loadout ? loadout.primary : null;
            var alternate = loadout ? loadout.alternate : null;

            container.Bind<IShipLoadout>()
                .FromMethod(context => new WeaponLoadout(
                    primary ? primary.Create(context.Container) : null,
                    alternate ? alternate.Create(context.Container) : null))
                .AsSingle();

            if (primary) primary.BindStatus(container, c => Resolve(c, WeaponSlot.Primary));
            if (alternate) alternate.BindStatus(container, c => Resolve(c, WeaponSlot.Alternate));
        }

        private static IWeapon Resolve(DiContainer container, WeaponSlot slot) =>
            container.Resolve<IShipLoadout>().TryGetWeapon(slot, out var weapon)
                ? weapon
                : throw new InvalidOperationException($"The ship loadout has no {slot} weapon.");
    }
}

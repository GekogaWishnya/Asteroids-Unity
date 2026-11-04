using System;
using System.Collections.Generic;
using Asteroids.Core;

namespace Asteroids.Weapons
{
    public static class WeaponStatusContracts
    {
        /// <param name="primary">Null for an empty slot.</param>
        /// <param name="alternate">Null for an empty slot.</param>
        /// <exception cref="InvalidOperationException">
        /// A contract is null or not an interface, or two weapons of the loadout declare the same contract.
        /// </exception>
        public static IReadOnlyList<(Type Contract, WeaponSlot Slot)> Collect(
            IWeaponDefinition primary, IWeaponDefinition alternate)
        {
            var result = new List<(Type, WeaponSlot)>();
            var owners = new Dictionary<Type, IWeaponDefinition>();

            Add(primary, WeaponSlot.Primary);
            Add(alternate, WeaponSlot.Alternate);
            return result;

            void Add(IWeaponDefinition definition, WeaponSlot slot)
            {
                if (definition?.StatusContracts == null) return;

                foreach (var contract in definition.StatusContracts)
                {
                    if (contract is not { IsInterface: true })
                        throw new InvalidOperationException(
                            $"Weapon '{definition.Name}' declares a status contract that is not an interface.");

                    if (owners.TryGetValue(contract, out var owner))
                        throw new InvalidOperationException(
                            $"Weapons '{owner.Name}' and '{definition.Name}' both provide {contract.Name}: " +
                            "a loadout can carry one weapon per status.");

                    owners.Add(contract, definition);
                    result.Add((contract, slot));
                }
            }
        }

        /// <summary>The weapon in <paramref name="slot"/>, as <paramref name="contract"/>.</summary>
        /// <exception cref="ArgumentNullException">An argument is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// The slot is empty, or its weapon doesn't implement the contract.
        /// </exception>
        public static object Resolve(IShipLoadout loadout, WeaponSlot slot, Type contract)
        {
            if (loadout == null) throw new ArgumentNullException(nameof(loadout));
            if (contract == null) throw new ArgumentNullException(nameof(contract));

            if (!loadout.TryGetWeapon(slot, out var weapon))
                throw new InvalidOperationException($"The ship loadout has no {slot} weapon.");
            if (!contract.IsInstanceOfType(weapon))
                throw new InvalidOperationException(
                    $"The {slot} weapon was declared to provide {contract.Name}, but it doesn't.");

            return weapon;
        }
    }
}
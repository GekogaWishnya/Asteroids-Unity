using System;
using Asteroids.Core;

namespace Asteroids.Weapons
{
    public sealed class WeaponLoadout : IShipLoadout
    {
        private readonly IWeapon _primary;
        private readonly IWeapon _alternate;

        /// <param name="primary">Fired with Fire. Null leaves the slot empty.</param>
        /// <param name="alternate">Fired with Alt Fire. Null leaves the slot empty.</param>
        /// <exception cref="ArgumentException">Both slots hold the same weapon, which can be mounted only once.</exception>
        public WeaponLoadout(IWeapon primary, IWeapon alternate)
        {
            if (primary != null && ReferenceEquals(primary, alternate))
                throw new ArgumentException("A weapon can be in one slot only.", nameof(alternate));

            _primary = primary;
            _alternate = alternate;
        }

        /// <summary>A loadout with no weapons.</summary>
        public static WeaponLoadout Empty() => new(null, null);

        /// <exception cref="ArgumentOutOfRangeException"><paramref name="slot"/> is not a known slot.</exception>
        public bool TryGetWeapon(WeaponSlot slot, out IWeapon weapon)
        {
            weapon = slot switch
            {
                WeaponSlot.Primary => _primary,
                WeaponSlot.Alternate => _alternate,
                _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, "Unknown weapon slot.")
            };

            return weapon != null;
        }
    }
}

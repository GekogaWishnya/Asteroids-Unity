using System;
using System.Collections.Generic;
using Asteroids.Core;
using Zenject;

namespace Asteroids.Player
{
    /// <summary>Mounts every weapon of the loadout while attached, each with the trigger of its slot.</summary>
    public sealed class ShipArmament : IShipPart
    {
        private static readonly WeaponSlot[] Slots = { WeaponSlot.Primary, WeaponSlot.Alternate };

        private readonly List<Mount> _mounts = new(Slots.Length);
        private readonly PartAttachment _attachment = new("ship armament");

        /// <exception cref="ArgumentNullException">Any argument is null, or a trigger the loadout needs is.</exception>
        public ShipArmament(IShipLoadout loadout, IWeaponTriggers triggers, IShipControlLock controlLock)
        {
            if (loadout == null) throw new ArgumentNullException(nameof(loadout));
            if (triggers == null) throw new ArgumentNullException(nameof(triggers));
            if (controlLock == null) throw new ArgumentNullException(nameof(controlLock));

            foreach (var slot in Slots)
            {
                if (!loadout.TryGetWeapon(slot, out var weapon)) continue;

                var trigger = triggers.For(slot)
                              ?? throw new ArgumentNullException(nameof(triggers), $"There is no {slot} trigger.");
                _mounts.Add(new Mount(weapon, new GatedFireInput(trigger, controlLock)));
            }
        }

        public IDisposable Attach(IEntity ship)
        {
            if (ship == null) throw new ArgumentNullException(nameof(ship));

            var registrations = new Func<IDisposable>[_mounts.Count];
            for (var i = 0; i < _mounts.Count; i++)
            {
                var mount = _mounts[i];
                registrations[i] = () => mount.Weapon.Attach(ship, mount.Trigger);
            }

            return _attachment.Start(registrations);
        }

        private readonly struct Mount
        {
            public Mount(IWeapon weapon, IFireInput trigger)
            {
                Weapon = weapon;
                Trigger = trigger;
            }

            public IWeapon Weapon { get; }
            public IFireInput Trigger { get; }
        }

        public sealed class Factory : PlaceholderFactory<ShipArmament> { }
    }
}

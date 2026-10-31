using System;
using Asteroids.Core;
using Zenject;

namespace Asteroids.Player
{
    /// <summary>Mounts the ship's weapon while attached.</summary>
    public sealed class ShipArmament : IShipPart
    {
        private readonly IWeapon _weapon;
        private readonly IFireInput _trigger;
        private readonly PartAttachment _attachment = new("ship armament");

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public ShipArmament(IWeapon weapon, IFireInput trigger, IShipControlLock controlLock)
        {
            _weapon = weapon ?? throw new ArgumentNullException(nameof(weapon));
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));
            if (controlLock == null) throw new ArgumentNullException(nameof(controlLock));

            _trigger = new GatedFireInput(trigger, controlLock);
        }

        public IDisposable Attach(IEntity ship)
        {
            if (ship == null) throw new ArgumentNullException(nameof(ship));

            return _attachment.Start(new Func<IDisposable>[] { () => _weapon.Attach(ship, _trigger) });
        }

        public sealed class Factory : PlaceholderFactory<ShipArmament> { }
    }
}

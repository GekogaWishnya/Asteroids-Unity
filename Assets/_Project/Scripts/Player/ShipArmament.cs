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
        public ShipArmament(IWeapon weapon, IFireInput trigger)
        {
            _weapon = weapon ?? throw new ArgumentNullException(nameof(weapon));
            _trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
        }

        public IDisposable Attach(IEntity ship)
        {
            if (ship == null) throw new ArgumentNullException(nameof(ship));

            return _attachment.Start(new Func<IDisposable>[] { () => _weapon.Attach(ship, _trigger) });
        }

        public sealed class Factory : PlaceholderFactory<ShipArmament> { }
    }
}

using System;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Zenject;

namespace Asteroids.Player
{
    /// <summary>While attached, the ship is a damage source - it hurts whatever it rams.</summary>
    public sealed class ShipRam : IShipPart
    {
        private readonly ShipRamSource _source;
        private readonly IDamageRegistry _damageRegistry;
        private readonly PartAttachment _attachment = new("ship ram");

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public ShipRam(IShipRamConfig config, IDamageRegistry damageRegistry)
        {
            _source = new ShipRamSource(config);
            _damageRegistry = damageRegistry ?? throw new ArgumentNullException(nameof(damageRegistry));
        }

        public IDisposable Attach(IEntity ship)
        {
            if (ship == null) throw new ArgumentNullException(nameof(ship));

            return _attachment.Start(new Func<IDisposable>[] { () => _damageRegistry.RegisterSource(ship, _source) });
        }

        public sealed class Factory : PlaceholderFactory<ShipRam> { }
    }
}

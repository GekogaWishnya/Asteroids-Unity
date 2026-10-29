using System;
using Asteroids.Core;
using Asteroids.Core.IConfigs;

namespace Asteroids.Player
{
    public sealed class ShipRamSource : IDamageSource
    {
        private readonly IShipRamConfig _config;

        /// <exception cref="ArgumentNullException"><paramref name="config"/> is null.</exception>
        public ShipRamSource(IShipRamConfig config) =>
            _config = config ?? throw new ArgumentNullException(nameof(config));

        public int Damage => _config.RamDamage;
    }
}

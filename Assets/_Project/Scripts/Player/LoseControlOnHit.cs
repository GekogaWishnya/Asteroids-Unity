using System;
using Asteroids.Core;
using Asteroids.Core.IConfigs;

namespace Asteroids.Player
{
    /// <summary>
    /// <see cref="IDamageable"/> that takes the controls away from the player for
    /// <see cref="IControlLossConfig.ControlLossSeconds"/> on every hit it lets through.
    /// </summary>
    public sealed class LoseControlOnHit : IDamageable
    {
        private readonly IDamageable _inner;
        private readonly ShipControlLoss _controlLoss;
        private readonly IControlLossConfig _config;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public LoseControlOnHit(IDamageable inner, ShipControlLoss controlLoss, IControlLossConfig config)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _controlLoss = controlLoss ?? throw new ArgumentNullException(nameof(controlLoss));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <exception cref="ArgumentOutOfRangeException"><paramref name="amount"/> is not positive.</exception>
        public void TakeDamage(int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Damage must be positive.");

            _controlLoss.Lose(_config.ControlLossSeconds);
            _inner.TakeDamage(amount);
        }
    }
}

using System;
using Asteroids.Common;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Health;

namespace Asteroids.Player
{
    public sealed class PlayerHealth : IPlayerHealth, IDamageable, IHealable
    {
        private readonly HealthModel _model;

        /// <exception cref="ArgumentNullException"><paramref name="config"/> is null.</exception>
        public PlayerHealth(IShipHealthConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            _model = new HealthModel(config.MaxHealth);
        }

        public IObservableValue<int> Current => _model.Current;
        public IObservableValue<int> Max => _model.Max;
        public bool IsDead => _model.IsDead;

        public event Action Depleted
        {
            add => _model.Depleted += value;
            remove => _model.Depleted -= value;
        }

        public void TakeDamage(int amount) => _model.TakeDamage(amount);
        public void Heal(int amount) => _model.Heal(amount);
        public void Reset() => _model.Reset();
    }
}

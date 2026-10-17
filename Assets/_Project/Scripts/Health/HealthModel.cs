using System;
using Asteroids.Common;
using Asteroids.Core;

namespace Asteroids.Health
{
    /// <summary>
    /// <para>
    /// Health of anything that can be hurt. Starts full and is dead when <see cref="IHealth.Current"/> reaches zero.
    /// </para>
    /// <para>
    /// <see cref="IHealth.Depleted"/> is raised when model dies. A dead model ignores <see cref="TakeDamage"/> and
    /// <see cref="Heal"/>. <see cref="Reset"/> brings it back alive with max health.
    /// </para>
    /// </summary>
    public sealed class HealthModel : IHealth, IDamageable, IHealable
    {
        private readonly ObservableValue<int> _current;
        private readonly ObservableValue<int> _max;

        /// <param name="max">Initial and maximum health.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="max"/> is not positive.</exception>
        public HealthModel(int max)
        {
            if (max <= 0) throw new ArgumentOutOfRangeException(nameof(max), "Max health must be positive.");

            _max = new ObservableValue<int>(max);
            _current = new ObservableValue<int>(max);
        }

        public IObservableValue<int> Current => _current;
        public IObservableValue<int> Max => _max;
        public bool IsDead => _current.Value == 0;

        public event Action Depleted;

        /// <summary>
        /// Lowers health, but never below zero. Does nothing if already dead.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="amount"/> is not positive.</exception>
        public void TakeDamage(int amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Damage must be positive.");
            if (IsDead) return;

            try
            {
                _current.Value = Math.Max(0, _current.Value - amount);
            }
            finally
            {
                if (IsDead) Depleted?.Invoke();
            }
        }

        /// <summary>Restores health, but never above <see cref="IHealth.Max"/>. Does nothing if dead.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="amount"/> is not positive.</exception>
        public void Heal(int amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Healing must be positive.");
            if (IsDead) return;

            var healed = (long)_current.Value + amount;
            _current.Value = (int)Math.Min(healed, _max.Value);
        }

        /// <summary>Refills health and revives a dead model.</summary>
        public void Reset() => _current.Value = _max.Value;
    }
}

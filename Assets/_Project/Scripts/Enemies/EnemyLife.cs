using System;
using System.Collections.Generic;
using Asteroids.Common;
using Asteroids.Health;
using UnityEngine;

namespace Asteroids.Enemies
{
    /// <summary>
    /// The time in play of one pooled enemy object, shared by every enemy controller. <see cref="Enter"/> refills
    /// the health, creates the enemy's registrations and shows the view.
    /// </summary>
    public sealed class EnemyLife : IDisposable
    {
        private readonly string _name;
        private readonly IEnemyView _view;
        private readonly HealthModel _health;
        private readonly IEnemyDeathHandler _death;
        private readonly RegistrationScope _registrations;

        /// <param name="name">What the enemy is called in error messages, such as "asteroid".</param>
        /// <param name="registrations">What the enemy registers while in play, in order. Copied.</param>
        /// <exception cref="ArgumentNullException">Any argument (or the view's entity, or a registration) is null.</exception>
        public EnemyLife(
            string name,
            IEnemyView view,
            HealthModel health,
            IEnemyDeathHandler death,
            IEnumerable<Func<IDisposable>> registrations)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _health = health ?? throw new ArgumentNullException(nameof(health));
            _death = death ?? throw new ArgumentNullException(nameof(death));
            if (registrations == null) throw new ArgumentNullException(nameof(registrations));
            if (view.Entity == null) throw new ArgumentNullException(nameof(view), "The view has no entity.");

            // Last, so it is the first to go when the enemy leaves play.
            var all = new List<Func<IDisposable>>(registrations) { ListenForDepletion };
            _registrations = new RegistrationScope(all);
        }

        /// <summary>Raised once per life, when the enemy is out of play again and can be reused.</summary>
        public event Action Finished;

        public bool IsInPlay => _registrations.IsEnabled;

        /// <summary>Puts the enemy in play.</summary>
        /// <param name="angle">Degrees, same convention as <see cref="Asteroids.Core.IPose.Angle"/>.</param>
        /// <param name="afterShown">Sets the enemy's motion for the new life.</param>
        /// <exception cref="InvalidOperationException">The enemy is already in play.</exception>
        public void Enter(Vector2 position, float angle, Action afterShown = null)
        {
            if (_registrations.IsEnabled)
                throw new InvalidOperationException($"The {_name} is already in play.");

            _health.Reset();
            _registrations.Enable();

            try
            {
                _view.Show(position, angle);
                afterShown?.Invoke();
            }
            catch
            {
                try
                {
                    Leave();
                }
                catch
                {
                    // The failure that led here matters more than a failure of the cleanup.
                }

                throw;
            }
        }
        
        public void Dispose()
        {
            Finished = null;

            try
            {
                _registrations.Dispose();
            }
            finally
            {
                _view.Dispose();
            }
        }

        private IDisposable ListenForDepletion()
        {
            _health.Depleted += OnDepleted;
            return new DepletionSubscription(_health, OnDepleted);
        }

        private void OnDepleted()
        {
            var position = _view.Entity.Pose.Position;

            try
            {
                _death.Died(position);
            }
            finally
            {
                Retire();
            }
        }

        private void Retire()
        {
            try
            {
                Leave();
            }
            finally
            {
                Finished?.Invoke();
            }
        }

        private void Leave()
        {
            try
            {
                _registrations.Disable();
            }
            finally
            {
                _view.Hide();
            }
        }

        private sealed class DepletionSubscription : IDisposable
        {
            private readonly HealthModel _health;
            private readonly Action _handler;

            public DepletionSubscription(HealthModel health, Action handler)
            {
                _health = health;
                _handler = handler;
            }

            public void Dispose() => _health.Depleted -= _handler;
        }
    }
}

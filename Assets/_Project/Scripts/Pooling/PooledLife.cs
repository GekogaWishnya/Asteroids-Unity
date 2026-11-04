using System;
using System.Collections.Generic;
using Asteroids.Common;
using Asteroids.Core;
using UnityEngine;

namespace Asteroids.Pooling
{
    /// <summary>
    /// The time in play of one pooled entity object, shared by enemies and projectiles.
    /// <see cref="Enter"/> creates the registrations, shows the view and lets the caller set the motion.
    /// <see cref="Retire"/> takes everything down again and raises <see cref="Finished"/>.
    /// </summary>
    public sealed class PooledLife : IDisposable
    {
        private readonly string _name;
        private readonly IPooledView _view;
        private readonly RegistrationScope _registrations;

        /// <param name="name">What the entity is called in error messages, such as "asteroid".</param>
        /// <param name="registrations">What the entity registers while in play, in order. Copied.</param>
        /// <exception cref="ArgumentNullException">Any argument (or the view's entity, or a registration) is null.</exception>
        public PooledLife(string name, IPooledView view, IEnumerable<Func<IDisposable>> registrations)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            if (registrations == null) throw new ArgumentNullException(nameof(registrations));
            if (view.Entity == null) throw new ArgumentNullException(nameof(view), "The view has no entity.");

            var all = new List<Func<IDisposable>> { view.RegisterColliders };
            all.AddRange(registrations);
            _registrations = new RegistrationScope(all);
        }

        /// <summary>Raised once per life, when the entity is out of play again and can be reused.</summary>
        public event Action Finished;

        public bool IsInPlay => _registrations.IsEnabled;

        /// <summary>Puts the entity in play. If showing or <paramref name="afterShown"/> fails, it leaves play again.</summary>
        /// <param name="angle">Degrees, same convention as <see cref="IPose.Angle"/>.</param>
        /// <param name="afterShown">Sets the entity's motion for the new life.</param>
        /// <exception cref="InvalidOperationException">The entity is already in play.</exception>
        public void Enter(Vector2 position, float angle, Action afterShown = null)
        {
            if (_registrations.IsEnabled)
                throw new InvalidOperationException($"The {_name} is already in play.");

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

        public void Retire()
        {
            if (!_registrations.IsEnabled) return;

            try
            {
                Leave();
            }
            finally
            {
                Finished?.Invoke();
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
    }
}
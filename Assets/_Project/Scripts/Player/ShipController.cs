using System;
using System.Collections.Generic;
using Asteroids.Common;
using Asteroids.Core;

namespace Asteroids.Player
{
    /// <summary>
    /// <para>
    /// Switches a ship on and off. Only listens to the depletion of the player’s health itself while
    /// the ship's behavior lives in its <see cref="IShipPart"/>s.
    /// </para>
    /// <para>
    /// When health runs out, the controller disables itself first, so every part is detached (protection ends,
    /// colliders are restored, views are told, the weapon is taken off), and then reports
    /// <see cref="IShipEventSink.Destroyed"/>. A dead ship cannot be enabled until its health is reset.
    /// </para>
    /// </summary>
    public sealed class ShipController : IShipController
    {
        private readonly RegistrationScope _registrations;
        private readonly IHealth _health;
        private readonly IShipEventSink _events;
        private readonly IEntity _entity;

        /// <param name="parts">Copied. Attached in this order.</param>
        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="parts"/> contains null.</exception>
        public ShipController(IEntity entity, IEnumerable<IShipPart> parts, IPlayerHealth health, IShipEventSink events)
        {
            _entity = entity ?? throw new ArgumentNullException(nameof(entity));
            _health = health ?? throw new ArgumentNullException(nameof(health));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            if (parts == null) throw new ArgumentNullException(nameof(parts));

            var registrations = new List<Func<IDisposable>>();
            foreach (var part in parts)
            {
                if (part == null) throw new ArgumentException("A ship part must not be null.", nameof(parts));
                registrations.Add(() => part.Attach(entity));
            }

            // Last, so it is the first to go when the ship is disabled.
            registrations.Add(ListenForDepletion);
            _registrations = new RegistrationScope(registrations);
        }

        public void Enable()
        {
            if (_health.IsDead) return;

            _registrations.Enable();
        }

        public void Disable() => _registrations.Disable();

        private IDisposable ListenForDepletion()
        {
            _health.Depleted += OnDepleted;
            return new DepletionSubscription(_health, OnDepleted);
        }

        private void OnDepleted()
        {
            Disable();
            _events.Destroyed(_entity);
        }

        private sealed class DepletionSubscription : IDisposable
        {
            private readonly IHealth _health;
            private readonly Action _handler;

            public DepletionSubscription(IHealth health, Action handler)
            {
                _health = health;
                _handler = handler;
            }

            public void Dispose() => _health.Depleted -= _handler;
        }
    }
}

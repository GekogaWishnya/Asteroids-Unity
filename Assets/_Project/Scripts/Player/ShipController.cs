using System;
using System.Collections.Generic;
using System.Linq;
using Asteroids.Common;
using Asteroids.Core;
using UnityEngine;

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
        private readonly PlayerHealth _health;
        private readonly IShipEventSink _events;
        private readonly IEntity _entity;
        private readonly IShipPart[] _parts;
        private readonly Vector2 _startPosition;
        private readonly float _startAngle;
        private bool _isWanted;

        /// <param name="parts">Copied. Attached in this order.</param>
        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="parts"/> contains null.</exception>
        public ShipController(IEntity entity, IEnumerable<IShipPart> parts, PlayerHealth health, IShipEventSink events)
        {
            _entity = entity ?? throw new ArgumentNullException(nameof(entity));
            if (entity.Pose == null) throw new ArgumentNullException(nameof(entity), "The ship has no pose.");
            _health = health ?? throw new ArgumentNullException(nameof(health));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            if (parts == null) throw new ArgumentNullException(nameof(parts));

            _parts = parts.ToArray();
            var registrations = new List<Func<IDisposable>>();
            foreach (var part in _parts)
            {
                if (part == null) throw new ArgumentException("A ship part must not be null.", nameof(parts));
                registrations.Add(() => part.Attach(entity));
            }

            _startPosition = entity.Pose.Position;
            _startAngle = entity.Pose.Angle;

            // Last, so it is the first to go when the ship is disabled.
            registrations.Add(ListenForDepletion);
            _registrations = new RegistrationScope(registrations);
        }

        public void Enable()
        {
            _isWanted = true;
            if (_health.IsDead) return;

            _registrations.Enable();
        }

        public void Disable()
        {
            _isWanted = false;
            _registrations.Disable();
        }

        public void Restart()
        {
            _registrations.Disable();

            var failures = new FailureCollector();
            foreach (var part in _parts)
            {
                try
                {
                    part.Reset();
                }
                catch (Exception e)
                {
                    failures.Record(e);
                }
            }

            var pose = _entity.Pose;
            pose.Position = _startPosition;
            pose.Angle = _startAngle;
            _health.Reset();

            if (_isWanted)
            {
                try
                {
                    _registrations.Enable();
                }
                catch (Exception e)
                {
                    failures.Record(e);
                }
            }

            failures.ThrowIfFailed();
        }

        private IDisposable ListenForDepletion() => _health.SubscribeDepleted(OnDepleted);

        private void OnDepleted()
        {
            _registrations.Disable();
            _events.Destroyed(_entity);
        }
    }
}
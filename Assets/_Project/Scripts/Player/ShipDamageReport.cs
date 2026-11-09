using System;
using Asteroids.Core;

namespace Asteroids.Player
{
    /// <summary>
    /// Reports every loss of the player's health to <see cref="IShipEventSink.Damaged"/> while attached.
    /// </summary>
    public sealed class ShipDamageReport : IShipPart
    {
        private readonly IPlayerHealth _health;
        private readonly IShipEventSink _events;
        private readonly PartAttachment _attachment = new("ship damage report");

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public ShipDamageReport(IPlayerHealth health, IShipEventSink events)
        {
            _health = health ?? throw new ArgumentNullException(nameof(health));
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public IDisposable Attach(IEntity ship)
        {
            if (ship == null) throw new ArgumentNullException(nameof(ship));

            return _attachment.Start(new Func<IDisposable>[]
            {
                () => _health.SubscribeDropped(isFatal => _events.Damaged(ship, isFatal))
            });
        }

        public void Reset() { }
    }
}
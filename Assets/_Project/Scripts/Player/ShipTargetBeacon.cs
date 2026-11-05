using System;
using Asteroids.Core;
using Asteroids.Physics;

namespace Asteroids.Player
{
    /// <summary>
    /// While attached, the ship is the <see cref="IPlayerTarget"/> that enemies hunt, except while it is protected.
    /// </summary>
    public sealed class ShipTargetBeacon : IShipPart
    {
        private readonly ShipTracker _tracker;
        private readonly IMotionState _motion;
        private readonly IShipProtectionState _protection;
        private readonly PartAttachment _attachment = new("ship target beacon");

        /// <param name="motion">The ship's motion.</param>
        /// <param name="protection">The ship's protection.</param>
        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public ShipTargetBeacon(ShipTracker tracker, IMotionState motion, IShipProtectionState protection)
        {
            _tracker = tracker ?? throw new ArgumentNullException(nameof(tracker));
            _motion = motion ?? throw new ArgumentNullException(nameof(motion));
            _protection = protection ?? throw new ArgumentNullException(nameof(protection));
        }

        public IDisposable Attach(IEntity ship)
        {
            if (ship == null) throw new ArgumentNullException(nameof(ship));

            return _attachment.Start(new Func<IDisposable>[] { () => _tracker.Track(ship.Pose, _motion, _protection) });
        }
    }
}
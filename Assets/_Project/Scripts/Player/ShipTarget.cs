using System;
using Asteroids.Core;
using UnityEngine;

namespace Asteroids.Player
{
    /// <summary>The player's ship as a target for enemies.</summary>
    public sealed class ShipTarget : IPlayerTarget
    {
        private readonly ShipTracker _tracker;

        /// <exception cref="ArgumentNullException"><paramref name="tracker"/> is null.</exception>
        public ShipTarget(ShipTracker tracker) =>
            _tracker = tracker ?? throw new ArgumentNullException(nameof(tracker));

        public bool HasTarget => TryGetTarget(out _);
        public Vector2 Position => TryGetTarget(out var ship) ? ship.Pose.Position : Vector2.zero;
        public Vector2 Velocity => TryGetTarget(out var ship) ? ship.Motion.Velocity : Vector2.zero;

        private bool TryGetTarget(out TrackedShip ship) =>
            _tracker.TryGetShip(out ship) && !ship.Protection.IsProtected;
    }
}
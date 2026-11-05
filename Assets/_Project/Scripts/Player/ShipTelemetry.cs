using System;
using Asteroids.Core;
using UnityEngine;

namespace Asteroids.Player
{
    /// <summary>The tracked ship's readings for the HUD.</summary>
    public sealed class ShipTelemetry : IShipTelemetry
    {
        private readonly ShipTracker _tracker;

        /// <exception cref="ArgumentNullException"><paramref name="tracker"/> is null.</exception>
        public ShipTelemetry(ShipTracker tracker) =>
            _tracker = tracker ?? throw new ArgumentNullException(nameof(tracker));

        public bool HasShip => _tracker.TryGetShip(out _);
        public Vector2 Position => _tracker.TryGetShip(out var ship) ? ship.Pose.Position : Vector2.zero;
        public float Angle => _tracker.TryGetShip(out var ship) ? ship.Pose.Angle : 0f;
        public float Speed => _tracker.TryGetShip(out var ship) ? ship.Motion.Speed : 0f;
    }
}
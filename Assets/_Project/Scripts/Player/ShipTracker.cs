using System;
using Asteroids.Core;
using Asteroids.Physics;

namespace Asteroids.Player
{
    /// <summary>
    /// In play ship reference. <see cref="ShipTargetBeacon"/> feeds it while the ship is enabled.
    /// Each reader decides how to see the ship's data.
    /// </summary>
    public sealed class ShipTracker
    {
        private Tracking _live;

        /// <returns>False when no ship is tracked.</returns>
        public bool TryGetShip(out TrackedShip ship)
        {
            ship = _live != null ? new TrackedShip(_live.Pose, _live.Motion, _live.Protection) : default;
            return _live != null;
        }

        /// <returns>Dispose to stop tracking the ship.</returns>
        /// <exception cref="ArgumentNullException">An argument is null.</exception>
        /// <exception cref="InvalidOperationException">A ship is already tracked.</exception>
        public IDisposable Track(IPose pose, IMotionState motion, IShipProtectionState protection)
        {
            if (pose == null) throw new ArgumentNullException(nameof(pose));
            if (motion == null) throw new ArgumentNullException(nameof(motion));
            if (protection == null) throw new ArgumentNullException(nameof(protection));
            if (_live != null) throw new InvalidOperationException("A ship is already tracked.");

            return _live = new Tracking(this, pose, motion, protection);
        }

        private sealed class Tracking : IDisposable
        {
            private readonly ShipTracker _owner;

            public Tracking(ShipTracker owner, IPose pose, IMotionState motion, IShipProtectionState protection)
            {
                _owner = owner;
                Pose = pose;
                Motion = motion;
                Protection = protection;
            }

            public IPose Pose { get; }
            public IMotionState Motion { get; }
            public IShipProtectionState Protection { get; }

            public void Dispose()
            {
                if (ReferenceEquals(_owner._live, this)) _owner._live = null;
            }
        }
    }

    public readonly struct TrackedShip
    {
        public TrackedShip(IPose pose, IMotionState motion, IShipProtectionState protection)
        {
            Pose = pose;
            Motion = motion;
            Protection = protection;
        }

        public IPose Pose { get; }
        public IMotionState Motion { get; }
        public IShipProtectionState Protection { get; }
    }
}
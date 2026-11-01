using System;
using Asteroids.Core;
using Asteroids.Physics;
using UnityEngine;

namespace Asteroids.Player
{
    /// <summary>The player's ship as a target for enemies.</summary>
    public sealed class ShipTarget : IPlayerTarget
    {
        private Tracking _live;

        public bool HasTarget => _live != null && !_live.Protection.IsProtected;
        public Vector2 Position => HasTarget ? _live.Pose.Position : Vector2.zero;
        public Vector2 Velocity => HasTarget ? _live.Motion.Velocity : Vector2.zero;

        /// <summary>
        /// Makes the ship with <paramref name="pose"/> and <paramref name="motion"/> the target whenever
        /// <paramref name="protection"/> says it is not protected.
        /// </summary>
        /// <returns>Dispose to stop being the target.</returns>
        /// <exception cref="ArgumentNullException">An argument is null.</exception>
        /// <exception cref="InvalidOperationException">A ship is already tracked.</exception>
        public IDisposable Track(IPose pose, IMotionState motion, IShipProtectionState protection)
        {
            if (pose == null) throw new ArgumentNullException(nameof(pose));
            if (motion == null) throw new ArgumentNullException(nameof(motion));
            if (protection == null) throw new ArgumentNullException(nameof(protection));
            if (_live != null) throw new InvalidOperationException("A ship is already the player's target.");

            return _live = new Tracking(this, pose, motion, protection);
        }

        private sealed class Tracking : IDisposable
        {
            private readonly ShipTarget _owner;

            public Tracking(ShipTarget owner, IPose pose, IMotionState motion, IShipProtectionState protection)
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
}

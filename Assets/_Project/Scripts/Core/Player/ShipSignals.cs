using UnityEngine;

namespace Asteroids.Core
{
    /// <summary>
    /// Fired when the ship loses health. Blocked hits (protection) are ignored.
    /// On a fatal hit it is fired before <see cref="ShipDestroyed"/>.
    /// </summary>
    public readonly struct ShipDamaged
    {
        public ShipDamaged(Vector2 position, bool isFatal)
        {
            Position = position;
            IsFatal = isFatal;
        }

        /// <summary>Ship's position during the hit.</summary>
        public Vector2 Position { get; }

        public bool IsFatal { get; }
    }

    public readonly struct ShipDestroyed
    {
        public ShipDestroyed(IEntity entity) => Entity = entity;

        public IEntity Entity { get; }
    }
}
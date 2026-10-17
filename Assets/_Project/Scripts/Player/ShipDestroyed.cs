using Asteroids.Core;

namespace Asteroids.Player
{
    /// <summary>Fired once when the ship runs out of health.</summary>
    public readonly struct ShipDestroyed
    {
        public ShipDestroyed(IEntity entity) => Entity = entity;

        public IEntity Entity { get; }
    }
}

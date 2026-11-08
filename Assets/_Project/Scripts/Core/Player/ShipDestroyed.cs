namespace Asteroids.Core
{
    /// <summary>Fired once when the ship runs out of health.</summary>
    public readonly struct ShipDestroyed
    {
        public ShipDestroyed(IEntity entity) => Entity = entity;

        public IEntity Entity { get; }
    }
}
using Asteroids.Core;

namespace Asteroids.Player
{
    /// <summary>Where the ship fires its events. Keeps <see cref="ShipController"/> free of the signal bus.</summary>
    public interface IShipEventSink
    {
        void Destroyed(IEntity entity);
    }
}

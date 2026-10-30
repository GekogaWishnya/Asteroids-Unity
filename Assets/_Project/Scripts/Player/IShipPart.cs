using System;
using Asteroids.Core;

namespace Asteroids.Player
{
    /// <summary>
    /// One behavior of a ship (moving, being protected, ramming, firing). A part is made for one ship and
    /// holds that ship's state for its whole life. <see cref="ShipController"/> attaches it while the ship is enabled.
    /// </summary>
    public interface IShipPart
    {
        /// <summary>Starts the behavior for <paramref name="ship"/>. Dispose the result to stop it.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="ship"/> is null.</exception>
        /// <exception cref="InvalidOperationException">The part is already attached.</exception>
        IDisposable Attach(IEntity ship);
    }
}

using Asteroids.Core;
using Zenject;

namespace Asteroids.Player
{
    /// <summary>
    /// Creates the <see cref="IShipController"/> of one ship, given its entity and what the scene lends to
    /// its protection.
    /// </summary>
    public sealed class ShipControllerFactory : PlaceholderFactory<IEntity, IProtectionTargets, IShipController> { }
}

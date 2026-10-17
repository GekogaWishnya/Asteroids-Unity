using Asteroids.Core;
using Zenject;

namespace Asteroids.Player
{
    /// <summary>Creates the <see cref="IShipController"/> of one ship, given its entity.</summary>
    public sealed class ShipControllerFactory : PlaceholderFactory<IEntity, IShipController> { }
}
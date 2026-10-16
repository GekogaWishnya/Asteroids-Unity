using Asteroids.Core;
using Zenject;

namespace Asteroids.Player
{
    /// <summary>
    /// Requires <see cref="Asteroids.Core.IMovementInput"/>, <see cref="Asteroids.Core.IConfigs.IShipMovementConfig"/>
    /// and <see cref="Asteroids.Core.ISimulationLoop"/> to be bound by the host installer.
    /// </summary>
    public sealed class PlayerInstaller : Installer<PlayerInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindFactory<IPose, IShipController, ShipControllerFactory>()
                .To<ShipController>();
        }
    }
}

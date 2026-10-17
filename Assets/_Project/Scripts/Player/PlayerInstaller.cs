using Asteroids.Core;
using Zenject;

namespace Asteroids.Player
{
    /// <summary>
    /// Requires <see cref="Asteroids.Core.IMovementInput"/>, <see cref="Asteroids.Core.IConfigs.IShipMovementConfig"/>,
    /// <see cref="Asteroids.Core.IConfigs.IShipHealthConfig"/>, <see cref="Asteroids.Core.ISimulationLoop"/> and
    /// <see cref="Asteroids.Core.IDamageRegistry"/> to be bound by the host installer, and
    /// <see cref="SignalBus"/> to be installed.
    /// </summary>
    public sealed class PlayerInstaller : Installer<PlayerInstaller>
    {
        public override void InstallBindings()
        {
            Container.DeclareSignal<ShipDestroyed>().OptionalSubscriber();
            
            Container.Bind<PlayerHealth>().AsSingle();
            Container.Bind<IPlayerHealth>().To<PlayerHealth>().FromResolve();

            Container.Bind<IShipEventSink>().To<SignalBusShipEventSink>().AsSingle();

            Container.BindFactory<IEntity, IShipController, ShipControllerFactory>()
                .To<ShipController>();
        }
    }
}
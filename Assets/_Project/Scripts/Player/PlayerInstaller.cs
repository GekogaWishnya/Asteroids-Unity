using Asteroids.Core;
using Zenject;

namespace Asteroids.Player
{
    /// <summary>
    /// Requires <see cref="Asteroids.Core.IMovementInput"/>, <see cref="Asteroids.Core.IFireInput"/>,
    /// <see cref="Asteroids.Core.IWeapon"/>, <see cref="Asteroids.Core.IConfigs.IShipMovementConfig"/>,
    /// <see cref="Asteroids.Core.IConfigs.IShipHealthConfig"/>, <see cref="Asteroids.Core.IConfigs.IShipRamConfig"/>,
    /// <see cref="Asteroids.Core.IConfigs.IInvulnerabilityConfig"/> (bound once, for the ship),
    /// <see cref="Asteroids.Core.IConfigs.IWaveProtectionConfig"/>, <see cref="Asteroids.Core.IConfigs.IControlLossConfig"/>,
    /// <see cref="Asteroids.Core.ISimulationLoop"/>, <see cref="Asteroids.Core.IDamageRegistry"/> and
    /// <see cref="Asteroids.Core.IKineticBodyRegistry"/> to be bound by the host installer, and
    /// <see cref="SignalBus"/> to be installed. Subscribes to <see cref="WaveStarted"/>, which another installer
    /// declares; the order of installation does not matter.
    /// </summary>
    public sealed class PlayerInstaller : Installer<PlayerInstaller>
    {
        public override void InstallBindings()
        {
            Container.DeclareSignal<ShipDestroyed>().OptionalSubscriber();

            Container.Bind<PlayerHealth>().AsSingle();
            Container.Bind<IPlayerHealth>().To<PlayerHealth>().FromResolve();
            
            Container.Bind(typeof(ShipControlLoss), typeof(IShipControlLock)).To<ShipControlLoss>().AsSingle();
            Container.Bind<IDamageable>().To<LoseControlOnHit>().AsSingle().WhenInjectedInto<ShipDefense>();
            Container.Bind<IDamageable>().To<PlayerHealth>().FromResolve().WhenInjectedInto<LoseControlOnHit>();

            Container.Bind<IShipEventSink>().To<SignalBusShipEventSink>().AsSingle();

            Container.Bind<WaveStartProtection>().AsSingle();
            Container.BindSignal<WaveStarted>()
                .ToMethod<WaveStartProtection>(protection => protection.OnWaveStarted)
                .FromResolve();

            Container.BindFactory<ShipMovement, ShipMovement.Factory>();
            Container.BindFactory<IProtectionTargets, ShipDefense, ShipDefense.Factory>();
            Container.BindFactory<ShipRam, ShipRam.Factory>();
            Container.BindFactory<ShipArmament, ShipArmament.Factory>();

            Container.BindFactory<IEntity, IProtectionTargets, IShipController, ShipControllerFactory>()
                .FromFactory<ShipControllerAssembler>();
        }
    }
}

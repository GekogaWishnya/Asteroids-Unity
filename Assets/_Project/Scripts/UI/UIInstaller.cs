using Asteroids.Core;
using Zenject;

namespace Asteroids.UI
{
    /// <summary>
    /// Requires <see cref="IPlayerHealth"/>, <see cref="IScore"/>, <see cref="IShipTelemetry"/> and
    /// HUD config contracts to be bound.
    /// <see cref="ILaserStatus"/> is optional.
    /// View models are created when the first view asks for them and are disposed together with the container.
    /// <see cref="ShipTelemetryViewModel"/> is created at startup.
    /// </summary>
    public sealed class UIInstaller : Installer<UIInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<HealthViewModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<ScoreViewModel>().AsSingle();

            Container.BindInterfacesAndSelfTo<ShipTelemetryViewModel>().AsSingle();
            Container.BindInterfacesTo<ViewModelRefresher>().AsSingle();

            // Only exists when the loadout carries a laser.
            Container.BindInterfacesAndSelfTo<LaserViewModel>().AsSingle()
                .When(context => context.Container.HasBinding<ILaserStatus>());
        }
    }
}
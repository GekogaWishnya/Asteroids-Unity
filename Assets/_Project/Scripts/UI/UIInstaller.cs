using Asteroids.Core;
using Zenject;

namespace Asteroids.UI
{
    /// <summary>
    /// Requires <see cref="Asteroids.Core.IPlayerHealth"/> and <see cref="Asteroids.Core.IScore"/> to be bound.
    /// <see cref="Asteroids.Core.ILaserStatus"/> is optional.
    /// View models are created when the first view asks for them and are disposed together with the container.
    /// </summary>
    public sealed class UIInstaller : Installer<UIInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<HealthViewModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<ScoreViewModel>().AsSingle();
            
            // Only exists when the loadout carries a laser.
            Container.BindInterfacesAndSelfTo<LaserViewModel>().AsSingle()
                .When(context => context.Container.HasBinding<ILaserStatus>());
        }
    }
}

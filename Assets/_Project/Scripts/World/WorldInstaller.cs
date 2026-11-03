using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Physics;
using Zenject;

namespace Asteroids.World
{
    /// <summary>
    /// Requires <see cref="IWorldLayout"/> and <see cref="IHudLayout"/> to be bound by
    /// the host installer and <see cref="ISimulationLoop"/>.
    /// Binds <see cref="IWorldBounds"/>, <see cref="IViewLayout"/> and <see cref="IWrapRegistry"/>, and
    /// wraps registered poses in the <see cref="SimulationPhase.Constraint"/>.
    /// </summary>
    public sealed class WorldInstaller : Installer<WorldInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IWorldBounds>().To<WorldBounds>().AsSingle();
            Container.Bind<IViewLayout>().To<ViewLayout>().AsSingle();

            Container.Bind<WorldWrap>().AsSingle();
            Container.Bind<IWrapRegistry>().To<WorldWrap>().FromResolve();
            Container.BindInterfacesTo<WorldWrapRegistration>().AsSingle();
        }
    }
}

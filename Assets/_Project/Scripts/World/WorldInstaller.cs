using Asteroids.Collisions;
using Asteroids.Core;
using Asteroids.Physics;
using Zenject;

namespace Asteroids.World
{
    /// <summary>
    /// Requires <see cref="Asteroids.Core.IConfigs.IWorldLayout"/> and <see cref="Asteroids.Core.IConfigs.IHudLayout"/>
    /// to be bound by the host installer and <see cref="CollisionInstaller"/> to be installed.
    /// Binds <see cref="IWorldBounds"/> (the gameplay area) and
    /// <see cref="IViewLayout"/> (combination of the gameplay area and the HUD band).
    /// </summary>
    public sealed class WorldInstaller : Installer<WorldInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IWorldBounds>().To<WorldBounds>().AsSingle();
            Container.Bind<IViewLayout>().To<ViewLayout>().AsSingle();
            Container.BindInterfacesTo<WorldBoundary>().AsSingle();

            Container.Bind<WorldWrapHandler>().AsSingle();
            Container.BindSignal<CollisionEntered>()
                .ToMethod<WorldWrapHandler>(handler => handler.OnCollisionEntered)
                .FromResolve();
            Container.BindSignal<CollisionStayed>()
                .ToMethod<WorldWrapHandler>(handler => handler.OnCollisionStayed)
                .FromResolve();
        }
    }
}

using Asteroids.Collisions;
using Asteroids.Physics;
using Zenject;

namespace Asteroids.World
{
    /// <summary>
    /// Requires <see cref="Asteroids.Core.IConfigs.IWorldLayout"/> and <see cref="CollisionInstaller"/> to be installed.
    /// </summary>
    public sealed class WorldInstaller : Installer<WorldInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IWorldBounds>().To<WorldBounds>().AsSingle();
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

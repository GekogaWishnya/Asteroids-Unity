using Zenject;

namespace Asteroids.Collisions
{
    public sealed class CollisionInstaller : Installer<CollisionInstaller>
    {
        public override void InstallBindings()
        {
            Container.DeclareSignal<CollisionEntered>().OptionalSubscriber();
            Container.DeclareSignal<CollisionStayed>().OptionalSubscriber();
            Container.DeclareSignal<CollisionExited>().OptionalSubscriber();

            Container.Bind<ICollisionSystem>().To<CollisionSystem>().AsSingle();
            Container.Bind<IColliderFactory>().To<ColliderFactory>().AsSingle();
            Container.BindInterfacesTo<CollisionTicker>().AsSingle();
        }
    }
}
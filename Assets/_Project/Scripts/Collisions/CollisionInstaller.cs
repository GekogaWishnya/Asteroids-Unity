using Zenject;

namespace Asteroids.Collisions
{
    /// <summary>
    /// Requires <see cref="Asteroids.Core.ISimulationLoop"/> to be bound (see SimulationInstaller).
    /// </summary>
    public sealed class CollisionInstaller : Installer<CollisionInstaller>
    {
        public override void InstallBindings()
        {
            Container.DeclareSignal<CollisionEntered>().OptionalSubscriber();
            Container.DeclareSignal<CollisionStayed>().OptionalSubscriber();
            Container.DeclareSignal<CollisionExited>().OptionalSubscriber();

            Container.Bind<IShapeContactTester>().FromInstance(ShapeContacts.CreateTable());
            Container.Bind<ICollisionEventSink>().To<SignalBusCollisionEventSink>().AsSingle();
            Container.Bind<ICollisionSystem>().To<CollisionSystem>().AsSingle();
            Container.BindInterfacesTo<CollisionStepRegistration>().AsSingle();
        }
    }
}

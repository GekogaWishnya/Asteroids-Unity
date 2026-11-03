using Asteroids.Collisions.Response;
using Zenject;

namespace Asteroids.Collisions
{
    /// <summary>
    /// Requires <see cref="Asteroids.Core.ISimulationLoop"/> to be bound (see SimulationInstaller).
    /// Binds <see cref="Asteroids.Core.IKineticBodyRegistry"/>. Every contact is resolved physically
    /// (<see cref="BounceResolver"/>) before it is fired on the signal bus, so gameplay always reacts to a
    /// contact whose bounce has already happened.
    /// </summary>
    public sealed class CollisionInstaller : Installer<CollisionInstaller>
    {
        public override void InstallBindings()
        {
            Container.DeclareSignal<CollisionEntered>().OptionalSubscriber();
            Container.DeclareSignal<CollisionStayed>().OptionalSubscriber();
            Container.DeclareSignal<CollisionExited>().OptionalSubscriber();

            Container.BindInterfacesTo<KineticBodyRegistry>().AsSingle();
            Container.Bind<ICollisionResponse>().To<BounceResolver>().AsSingle();
            Container.Bind<SignalBusCollisionEventSink>().AsSingle();
            Container.Bind<ICollisionEventSink>().FromMethod(CreateSink).AsSingle();

            Container.Bind<IShapeContactTester>().FromInstance(ShapeContacts.CreateTable());
            Container.Bind<ICollisionSystem>().To<CollisionSystem>().AsSingle();
            Container.BindInterfacesTo<CollisionStepRegistration>().AsSingle();
        }

        /// <summary>
        /// Called on first resolve, after every installer ran.
        /// </summary>
        private static ICollisionEventSink CreateSink(InjectContext context)
        {
            var responses = CollisionResponses.InOrder(context.Container.ResolveAll<ICollisionResponse>());
            var sinks = new ICollisionEventSink[responses.Length + 1];
            responses.CopyTo(sinks, 0);
            sinks[responses.Length] = context.Container.Resolve<SignalBusCollisionEventSink>();
            return new CompositeCollisionEventSink(sinks);
        }
    }
}
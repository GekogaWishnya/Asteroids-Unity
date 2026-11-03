using System;
using Zenject;

namespace Asteroids.Collisions
{
    public sealed class SignalBusCollisionEventSink : ICollisionEventSink
    {
        private readonly SignalBus _signalBus;

        public SignalBusCollisionEventSink(SignalBus signalBus) =>
            _signalBus = signalBus ?? throw new ArgumentNullException(nameof(signalBus));

        public void Entered(ICollider first, ICollider second, in Contact contact) =>
            _signalBus.Fire(new CollisionEntered(first, second, contact));

        public void Stayed(ICollider first, ICollider second, in Contact contact) =>
            _signalBus.Fire(new CollisionStayed(first, second, contact));

        public void Exited(ICollider first, ICollider second) =>
            _signalBus.Fire(new CollisionExited(first, second));
    }
}

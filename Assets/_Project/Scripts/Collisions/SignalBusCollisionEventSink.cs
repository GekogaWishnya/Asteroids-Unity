using System;
using Zenject;

namespace Asteroids.Collisions
{
    public sealed class SignalBusCollisionEventSink : ICollisionEventSink
    {
        private readonly SignalBus _signalBus;

        public SignalBusCollisionEventSink(SignalBus signalBus) =>
            _signalBus = signalBus ?? throw new ArgumentNullException(nameof(signalBus));

        public void Entered(ICollider self, ICollider other, in Contact contact) =>
            _signalBus.Fire(new CollisionEntered(self, other, contact));

        public void Stayed(ICollider self, ICollider other, in Contact contact) =>
            _signalBus.Fire(new CollisionStayed(self, other, contact));

        public void Exited(ICollider self, ICollider other) =>
            _signalBus.Fire(new CollisionExited(self, other));
    }
}

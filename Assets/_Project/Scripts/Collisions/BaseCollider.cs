using System;
using Zenject;

namespace Asteroids.Collisions
{
    public abstract class BaseCollider : ICollider
    {
        private readonly SignalBus _signalBus;

        protected BaseCollider(SignalBus signalBus, object owner, CollisionLayer layer, CollisionLayer mask)
        {
            _signalBus = signalBus ?? throw new ArgumentNullException(nameof(signalBus));
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Layer = layer;
            Mask = mask;
        }

        public object Owner { get; }
        public CollisionLayer Layer { get; }
        public CollisionLayer Mask { get; }

        public abstract bool TryGetContact(ICollider other, out Contact contact);

        public void NotifyEntered(ICollider other, in Contact contact) =>
            _signalBus.Fire(new CollisionEntered(this, other, contact));

        public void NotifyStayed(ICollider other, in Contact contact) =>
            _signalBus.Fire(new CollisionStayed(this, other, contact));

        public void NotifyExited(ICollider other) =>
            _signalBus.Fire(new CollisionExited(this, other));
    }
}

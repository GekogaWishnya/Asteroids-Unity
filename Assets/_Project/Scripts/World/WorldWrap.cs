using Asteroids.Collisions;
using UnityEngine;
using Zenject;

namespace Asteroids.World
{
    public class WorldWrap : MonoBehaviour
    {
        private SignalBus _signalBus;

        [Inject]
        private void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
            if (isActiveAndEnabled) Subscribe();
        }

        private void OnEnable()
        {
            if (_signalBus != null) Subscribe();
        }

        private void OnDisable() => Unsubscribe();

        private void Subscribe() => _signalBus.Subscribe<CollisionEntered>(Wrap);
        private void Unsubscribe() => _signalBus.Unsubscribe<CollisionEntered>(Wrap);

        private void Wrap(CollisionEntered signal)
        {
            if (!ReferenceEquals(signal.Self.Owner, gameObject)) return;
            if (signal.Other.Owner is not GameObject owner) return;
            owner.transform.position = Vector2.zero;
        }
    }
}

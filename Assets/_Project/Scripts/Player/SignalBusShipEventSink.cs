using System;
using Asteroids.Core;
using Zenject;

namespace Asteroids.Player
{
    public sealed class SignalBusShipEventSink : IShipEventSink
    {
        private readonly SignalBus _signalBus;

        public SignalBusShipEventSink(SignalBus signalBus) =>
            _signalBus = signalBus ?? throw new ArgumentNullException(nameof(signalBus));

        public void Destroyed(IEntity entity) => _signalBus.Fire(new ShipDestroyed(entity));
    }
}

using System;
using Asteroids.Core;
using Zenject;

namespace Asteroids.Session
{
    public sealed class SignalBusRunEventSink : IRunEventSink
    {
        private readonly SignalBus _signalBus;

        public SignalBusRunEventSink(SignalBus signalBus) =>
            _signalBus = signalBus ?? throw new ArgumentNullException(nameof(signalBus));

        public void Ended(RunResult result) => _signalBus.Fire(new RunEnded(result));
    }
}

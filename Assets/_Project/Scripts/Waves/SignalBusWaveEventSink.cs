using System;
using Asteroids.Core;
using Zenject;

namespace Asteroids.Waves
{
    public sealed class SignalBusWaveEventSink : IWaveEventSink
    {
        private readonly SignalBus _signalBus;

        public SignalBusWaveEventSink(SignalBus signalBus) =>
            _signalBus = signalBus ?? throw new ArgumentNullException(nameof(signalBus));

        public void Started(int number, int total) => _signalBus.Fire(new WaveStarted(number, total));

        public void Cleared(int number) => _signalBus.Fire(new WaveCleared(number));

        public void Completed(int total) => _signalBus.Fire(new LevelCompleted(total));
    }
}

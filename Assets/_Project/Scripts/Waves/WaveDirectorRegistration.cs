using System;
using Asteroids.Core;
using Zenject;

namespace Asteroids.Waves
{
    /// <summary>
    /// Plugs the wave director into the simulation loop.
    /// Creating it creates the director, which validates the level.
    /// </summary>
    public sealed class WaveDirectorRegistration : IInitializable, IDisposable
    {
        private readonly ISimulationLoop _loop;
        private readonly WaveDirector _director;
        private IDisposable _registration;

        public WaveDirectorRegistration(ISimulationLoop loop, WaveDirector director)
        {
            _loop = loop ?? throw new ArgumentNullException(nameof(loop));
            _director = director ?? throw new ArgumentNullException(nameof(director));
        }

        public void Initialize() => _registration = _loop.Register(SimulationPhase.Logic, _director);

        public void Dispose() => _registration?.Dispose();
    }
}

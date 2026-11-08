using System;
using Asteroids.Core;
using Zenject;

namespace Asteroids.Session
{
    /// <summary>Plugs the <see cref="RunLifecycle"/> into the simulation loop.</summary>
    public sealed class RunLifecycleRegistration : IInitializable, IDisposable
    {
        private readonly ISimulationLoop _loop;
        private readonly RunLifecycle _lifecycle;
        private IDisposable _registration;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public RunLifecycleRegistration(ISimulationLoop loop, RunLifecycle lifecycle)
        {
            _loop = loop ?? throw new ArgumentNullException(nameof(loop));
            _lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));
        }

        public void Initialize() => _registration = _loop.Register(SimulationPhase.Logic, _lifecycle);

        public void Dispose() => _registration?.Dispose();
    }
}

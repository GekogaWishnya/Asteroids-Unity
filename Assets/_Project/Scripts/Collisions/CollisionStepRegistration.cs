using System;
using Asteroids.Core;
using Zenject;

namespace Asteroids.Collisions
{
    /// <summary>Plugs the collision system into the simulation loop for the lifetime of the container.</summary>
    public sealed class CollisionStepRegistration : IInitializable, IDisposable
    {
        private readonly ISimulationLoop _loop;
        private readonly ICollisionSystem _system;
        private IDisposable _registration;

        public CollisionStepRegistration(ISimulationLoop loop, ICollisionSystem system)
        {
            _loop = loop ?? throw new ArgumentNullException(nameof(loop));
            _system = system ?? throw new ArgumentNullException(nameof(system));
        }

        public void Initialize() => _registration = _loop.Register(SimulationPhase.Collision, _system);

        public void Dispose() => _registration?.Dispose();
    }
}

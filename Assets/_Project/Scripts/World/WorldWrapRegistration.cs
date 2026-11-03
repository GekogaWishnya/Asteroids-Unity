using System;
using Asteroids.Core;
using Zenject;

namespace Asteroids.World
{
    public sealed class WorldWrapRegistration : IInitializable, IDisposable
    {
        private readonly ISimulationLoop _loop;
        private readonly WorldWrap _wrap;
        private IDisposable _registration;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public WorldWrapRegistration(ISimulationLoop loop, WorldWrap wrap)
        {
            _loop = loop ?? throw new ArgumentNullException(nameof(loop));
            _wrap = wrap ?? throw new ArgumentNullException(nameof(wrap));
        }

        public void Initialize() => _registration ??= _loop.Register(SimulationPhase.Constraint, _wrap);

        public void Dispose()
        {
            _registration?.Dispose();
            _registration = null;
        }
    }
}

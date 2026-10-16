using Asteroids.Core;

namespace Asteroids.Collisions
{
    /// <summary>
    /// Detects overlaps between registered colliders.
    /// Each <see cref="ISimulationStep.Tick"/> detects overlaps and reports Enter/Stay/Exit signals.
    /// </summary>
    public interface ICollisionSystem : ISimulationStep
    {
        /// <summary>Throws if already registered. Deferred to the end of the tick if called during a tick.</summary>
        /// <exception cref="System.ArgumentNullException">Collider is null.</exception>
        /// <exception cref="System.ArgumentException">Collider has no shape.</exception>
        /// <exception cref="System.InvalidOperationException">Already registered.</exception>
        /// <exception cref="System.NotSupportedException">Collision between given shapes is not supported</exception>
        void Register(ICollider collider);

        /// <summary>
        /// Returns false if not registered. After being unregistered collider gets Exit signal.
        /// Deferred to the end of the tick if called during a tick. The collider still gets this tick's events.
        /// </summary>
        bool Unregister(ICollider collider);
    }
}

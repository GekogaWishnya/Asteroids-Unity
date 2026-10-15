using Asteroids.Core;

namespace Asteroids.Collisions
{
    /// <summary>
    /// Detects overlaps between registered colliders.
    /// Each <see cref="ISimulationStep.Tick"/> detects overlaps and fires Enter/Stay/Exit signals.
    /// </summary>
    public interface ICollisionSystem : ISimulationStep
    {
        /// <summary>Throws if already registered. Deferred to the end of the tick if called during a tick.</summary>
        void Register(ICollider collider);

        /// <summary>
        /// Returns false if not registered. After being unregistered collider gets Exit signal.
        /// Deferred to the end of the tick if called during a tick. The collider still gets this tick's events.
        /// </summary>
        bool Unregister(ICollider collider);
    }
}

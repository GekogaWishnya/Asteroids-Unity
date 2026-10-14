using System;

namespace Asteroids.Collisions
{
    public interface ICollisionSystem
    {
        /// <summary>Throws if already registered. Deferred to the end of the step if called during <see cref="Step"/>.</summary>
        void Register(ICollider collider);

        /// <summary>
        /// Returns false if not registered. After being unregistered collider gets Exit signal.
        /// Deferred to the end of the step if called during <see cref="Step"/> Collider still gets this step's events.
        /// </summary>
        bool Unregister(ICollider collider);

        event Action BeforeStep;
        
        /// <summary>
        /// Detects overlaps and fires Enter/Stay/Exit signals.
        /// if called during <see cref="Step"/>, throws.
        /// </summary>
        void Step();
    }
}

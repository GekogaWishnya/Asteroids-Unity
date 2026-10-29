using Asteroids.Core.IConfigs;

namespace Asteroids.Core
{
    /// <summary>
    /// Lets collider ignore a set of layers for a set amount of time.
    /// </summary>
    public interface ICollisionIgnore
    {
        /// <summary>
        /// Removes <paramref name="layers"/> from what the collider reacts to, replacing any earlier call.
        /// Takes effect from the next collision tick.
        /// </summary>
        void Ignore(CollisionLayer layers);

        /// <summary>Undoes <see cref="Ignore"/>.</summary>
        void Restore();
    }
}

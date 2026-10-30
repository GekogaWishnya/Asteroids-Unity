using Asteroids.Core.IConfigs;

namespace Asteroids.Core
{
    /// <summary>
    /// Lets a collider ignore a set of layers until <see cref="Restore"/> is called.
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

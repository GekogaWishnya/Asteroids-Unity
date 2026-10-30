using System;

namespace Asteroids.Core
{
    /// <summary>
    /// Something an entity fires. Every kind of weapon is its own implementation,
    /// so a carrier never knows how the weapon it holds works.
    /// </summary>
    public interface IWeapon
    {
        /// <summary>
        /// Mounts the weapon on <paramref name="carrier"/>. From then on it fires from the carrier's pose whenever
        /// <paramref name="trigger"/> asks for it. Dispose the result to take it off again. What the weapon already
        /// fired is not affected by that. A weapon is mounted on one carrier at a time.
        /// </summary>
        /// <exception cref="ArgumentNullException">An argument is null.</exception>
        /// <exception cref="InvalidOperationException">The weapon is already attached.</exception>
        IDisposable Attach(IEntity carrier, IFireInput trigger);
    }
}

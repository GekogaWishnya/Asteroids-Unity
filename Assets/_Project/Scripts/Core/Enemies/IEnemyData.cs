using Asteroids.Core.IConfigs;

namespace Asteroids.Core
{
    /// <summary>
    /// Everything the rest of the game needs to know about enemies - what can be spawned and how many are alive.
    /// </summary>
    public interface IEnemyData
    {
        /// <summary>
        /// Enemies that were spawned and have not been destroyed or removed yet, split children included.
        /// </summary>
        int AliveCount { get; }

        /// <summary>Whether a spawner for <paramref name="type"/> is registered.</summary>
        bool CanSpawn(EnemyTypeId type);

        /// <exception cref="System.InvalidOperationException">No spawner is registered for <paramref name="type"/>.</exception>
        void Spawn(EnemyTypeId type);
    }
}

using Asteroids.Core.IConfigs;

namespace Asteroids.Core
{
    /// <summary>Everything the rest of the game needs to know about enemies.</summary>
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

        /// <summary>Takes every enemy out of play without triggering death events.</summary>
        void DespawnAll();
    }
}
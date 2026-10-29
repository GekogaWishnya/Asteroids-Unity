using Asteroids.Core.IConfigs;

namespace Asteroids.Enemies
{
    /// <summary>Spawns one type of enemy and knows how many of them are alive.</summary>
    public interface IEnemySpawner
    {
        EnemyTypeId Type { get; }

        /// <summary>Spawned and not yet destroyed, split children included.</summary>
        int AliveCount { get; }

        /// <summary>Puts one enemy in play. The enemy chooses its own place.</summary>
        void Spawn();
    }
}

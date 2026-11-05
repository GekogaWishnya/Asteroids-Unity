namespace Asteroids.Core.IConfigs
{
    public interface ISpawnEntryConfig
    {
        /// <summary>Distance in world units outside the gameplay area where enemies appear.</summary>
        float SpawnMargin { get; }

        /// <summary>Seconds an entering enemy may stay outside the play area before it's visible.</summary>
        float MaxEnteringSeconds { get; }
    }
}
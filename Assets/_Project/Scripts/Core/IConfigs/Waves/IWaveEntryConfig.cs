namespace Asteroids.Core.IConfigs
{
    /// <summary>A batch of one enemy type inside a wave.</summary>
    public interface IWaveEntryConfig
    {
        EnemyTypeId Type { get; }

        /// <summary>How many enemies the entry spawns. At least 1.</summary>
        int Count { get; }

        /// <summary>Seconds from the start of the wave until the first enemy of the entry appears.</summary>
        float StartDelay { get; }

        /// <summary>Seconds between two enemies of the entry. 0 spawns them all at once.</summary>
        float Interval { get; }
    }
}

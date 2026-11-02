using System.Collections.Generic;

namespace Asteroids.Core.IConfigs
{
    public interface IWaveConfig
    {
        /// <summary>
        /// Seconds to wait before this wave starts, counted from the moment the previous wave has fully
        /// spawned and nothing is alive any more, or from the start of the level for the first wave.
        /// </summary>
        float DelayBeforeStart { get; }

        IReadOnlyList<IWaveEntryConfig> Entries { get; }
    }
}

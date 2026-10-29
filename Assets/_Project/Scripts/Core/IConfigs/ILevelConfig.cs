using System.Collections.Generic;

namespace Asteroids.Core.IConfigs
{
    /// <summary>The ordered waves of a level.</summary>
    public interface ILevelConfig
    {
        IReadOnlyList<IWaveConfig> Waves { get; }
    }
}

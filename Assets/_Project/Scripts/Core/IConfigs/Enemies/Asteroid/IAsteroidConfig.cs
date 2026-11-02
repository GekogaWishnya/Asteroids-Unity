using System.Collections.Generic;

namespace Asteroids.Core.IConfigs
{
    public interface IAsteroidConfig
    {
        IReadOnlyList<IAsteroidTypeConfig> Types { get; }
    }
}

using System.Collections.Generic;

namespace Asteroids.Core.IConfigs
{
    public interface IUfoConfig
    {
        IReadOnlyList<IUfoTypeConfig> Types { get; }
    }
}

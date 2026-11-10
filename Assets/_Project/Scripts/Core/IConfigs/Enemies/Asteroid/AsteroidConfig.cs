using System;
using System.Collections.Generic;

namespace Asteroids.Core.IConfigs
{
    /// <summary>The asteroid config put together from the asteroid types in play.</summary>
    public sealed class AsteroidConfig : IAsteroidConfig
    {
        /// <exception cref="ArgumentNullException"><paramref name="types"/> is null.</exception>
        public AsteroidConfig(IEnumerable<IAsteroidTypeConfig> types)
        {
            if (types == null) throw new ArgumentNullException(nameof(types));
            Types = new List<IAsteroidTypeConfig>(types);
        }

        public IReadOnlyList<IAsteroidTypeConfig> Types { get; }
    }
}
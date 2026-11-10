using System;
using System.Collections.Generic;

namespace Asteroids.Core.IConfigs
{
    /// <summary>The UFO config put together from the UFO types in play.</summary>
    public sealed class UfoConfig : IUfoConfig
    {
        /// <exception cref="ArgumentNullException"><paramref name="types"/> is null.</exception>
        public UfoConfig(IEnumerable<IUfoTypeConfig> types)
        {
            if (types == null) throw new ArgumentNullException(nameof(types));
            Types = new List<IUfoTypeConfig>(types);
        }

        public IReadOnlyList<IUfoTypeConfig> Types { get; }
    }
}
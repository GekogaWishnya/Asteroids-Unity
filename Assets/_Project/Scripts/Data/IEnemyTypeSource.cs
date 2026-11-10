using System.Collections.Generic;

namespace Asteroids.Data
{
    public interface IEnemyTypeSource
    {
        /// <summary>Every enemy type the config refers to directly.</summary>
        IEnumerable<EnemyTypeDefinition> ReferencedTypes { get; }
    }
}
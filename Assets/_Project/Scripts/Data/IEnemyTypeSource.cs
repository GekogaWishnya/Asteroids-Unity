using System.Collections.Generic;

namespace Asteroids.Data
{
    public interface IEnemyTypeSource
    {
        /// <summary>Every enemy type the config refers to. Empty entries are skipped, repeats are allowed.</summary>
        IEnumerable<EnemyTypeDefinition> ReferencedTypes { get; }
    }
}
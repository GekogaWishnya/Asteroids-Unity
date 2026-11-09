using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Core
{
    /// <summary>
    /// Fired when an enemy loses health. On a fatal hit it is fired before <see cref="EnemyDied"/>.
    /// </summary>
    public readonly struct EnemyDamaged
    {
        public EnemyDamaged(EnemyTypeId type, Vector2 position, bool isFatal)
        {
            Type = type;
            Position = position;
            IsFatal = isFatal;
        }

        public EnemyTypeId Type { get; }
        public Vector2 Position { get; }
        public bool IsFatal { get; }
    }

    public readonly struct EnemyDied
    {
        public EnemyDied(EnemyTypeId type, Vector2 position)
        {
            Type = type;
            Position = position;
        }

        public EnemyTypeId Type { get; }
        public Vector2 Position { get; }
    }
}
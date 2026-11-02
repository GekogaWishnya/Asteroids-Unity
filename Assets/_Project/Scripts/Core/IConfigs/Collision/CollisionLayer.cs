using System;

namespace Asteroids.Core.IConfigs
{
    [Flags]
    public enum CollisionLayer
    {
        None             = 0,
        Player           = 1 << 0,
        Enemy            = 1 << 1,
        PlayerProjectile = 1 << 2,
        EnemyProjectile  = 1 << 3,
        Boundary         = 1 << 4,
        All              = ~0
    }
}

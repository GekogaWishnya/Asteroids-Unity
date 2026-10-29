using System;

namespace Asteroids.Core.IConfigs
{
    [Flags]
    public enum CollisionLayer
    {
        None       = 0,
        Player     = 1 << 0,
        Enemy      = 1 << 1,
        Projectile = 1 << 2,
        Boundary   = 1 << 3,
        All        = ~0
    }
}

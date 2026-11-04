using Asteroids.Core;

namespace Asteroids.Weapons.Projectiles
{
    public interface IProjectileViewFactory
    {
        IPooledView Create();
    }
}
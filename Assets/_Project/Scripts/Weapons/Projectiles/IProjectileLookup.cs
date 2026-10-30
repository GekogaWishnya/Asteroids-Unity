using Asteroids.Core;

namespace Asteroids.Weapons.Projectiles
{
    /// <summary>Hides the registering methods from the code that reacts to hits.</summary>
    public interface IProjectileLookup
    {
        bool TryGet(IEntity entity, out IProjectileImpact projectile);
    }
}

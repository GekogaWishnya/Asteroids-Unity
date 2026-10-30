using System;
using Asteroids.Core;

namespace Asteroids.Weapons.Projectiles
{
    public interface IProjectileRegistry
    {
        /// <summary>Dispose the returned handle to remove the registration.</summary>
        /// <exception cref="ArgumentNullException">An argument is null.</exception>
        /// <exception cref="InvalidOperationException">The entity is already registered.</exception>
        IDisposable Register(IEntity entity, IProjectileImpact projectile);
    }
}

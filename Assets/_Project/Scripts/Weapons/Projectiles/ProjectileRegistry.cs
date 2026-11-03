using System;
using Asteroids.Common;
using Asteroids.Core;

namespace Asteroids.Weapons.Projectiles
{
    public sealed class ProjectileRegistry : IProjectileRegistry, IProjectileLookup
    {
        private readonly RegistrationMap<IEntity, IProjectileImpact> _projectiles = new("projectile");

        public IDisposable Register(IEntity entity, IProjectileImpact projectile) =>
            _projectiles.Add(entity, projectile, nameof(entity), nameof(projectile));

        public bool TryGet(IEntity entity, out IProjectileImpact projectile) =>
            _projectiles.TryGet(entity, out projectile);
    }
}

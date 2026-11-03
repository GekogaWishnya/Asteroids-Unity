using System;
using Asteroids.Collisions;
using Asteroids.Core;

namespace Asteroids.Weapons.Projectiles
{
    /// <summary>
    /// Stops a projectile when it touches another entity.
    /// </summary>
    public sealed class ProjectileImpactHandler : ICollisionResponse
    {
        private readonly IProjectileLookup _lookup;

        /// <exception cref="ArgumentNullException"><paramref name="lookup"/> is null.</exception>
        public ProjectileImpactHandler(IProjectileLookup lookup) =>
            _lookup = lookup ?? throw new ArgumentNullException(nameof(lookup));

        public CollisionResponseOrder Order => CollisionResponseOrder.ProjectileImpact;

        public void Entered(ICollider first, ICollider second, in Contact contact)
        {
            if (first?.Owner is not IEntity firstEntity) return;
            if (second?.Owner is not IEntity secondEntity) return;

            try
            {
                ImpactIfProjectile(firstEntity);
            }
            finally
            {
                ImpactIfProjectile(secondEntity);
            }
        }

        public void Stayed(ICollider first, ICollider second, in Contact contact) { }

        public void Exited(ICollider first, ICollider second) { }

        private void ImpactIfProjectile(IEntity entity)
        {
            if (_lookup.TryGet(entity, out var projectile)) projectile.Impact();
        }
    }
}
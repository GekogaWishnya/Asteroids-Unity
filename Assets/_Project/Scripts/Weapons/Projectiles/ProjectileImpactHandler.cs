using System;
using Asteroids.Collisions;
using Asteroids.Core;

namespace Asteroids.Weapons.Projectiles
{
    /// <summary>
    /// Stops a projectile when it touches another entity. The world's walls are not entities, so a projectile wraps.
    /// </summary>
    public sealed class ProjectileImpactHandler
    {
        private readonly IProjectileLookup _lookup;

        /// <exception cref="ArgumentNullException"><paramref name="lookup"/> is null.</exception>
        public ProjectileImpactHandler(IProjectileLookup lookup) =>
            _lookup = lookup ?? throw new ArgumentNullException(nameof(lookup));

        public void OnCollisionEntered(CollisionEntered signal)
        {
            if (signal.Self?.Owner is not IEntity self) return;
            if (signal.Other?.Owner is not IEntity) return;
            if (!_lookup.TryGet(self, out var projectile)) return;

            projectile.Impact();
        }
    }
}

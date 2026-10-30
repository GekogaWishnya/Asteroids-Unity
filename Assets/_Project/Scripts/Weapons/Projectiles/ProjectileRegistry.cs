using System;
using System.Collections.Generic;
using Asteroids.Core;

namespace Asteroids.Weapons.Projectiles
{
    public sealed class ProjectileRegistry : IProjectileRegistry, IProjectileLookup
    {
        private readonly Dictionary<IEntity, IProjectileImpact> _projectiles = new();

        public IDisposable Register(IEntity entity, IProjectileImpact projectile)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (projectile == null) throw new ArgumentNullException(nameof(projectile));

            if (!_projectiles.TryAdd(entity, projectile))
                throw new InvalidOperationException("The entity is already registered as a projectile.");

            return new Registration(_projectiles, entity, projectile);
        }

        public bool TryGet(IEntity entity, out IProjectileImpact projectile)
        {
            if (entity == null)
            {
                projectile = null;
                return false;
            }

            return _projectiles.TryGetValue(entity, out projectile);
        }
        
        private sealed class Registration : IDisposable
        {
            private readonly Dictionary<IEntity, IProjectileImpact> _map;
            private readonly IEntity _entity;
            private readonly IProjectileImpact _projectile;

            public Registration(Dictionary<IEntity, IProjectileImpact> map, IEntity entity, IProjectileImpact projectile)
            {
                _map = map;
                _entity = entity;
                _projectile = projectile;
            }

            public void Dispose()
            {
                if (_map.TryGetValue(_entity, out var current) && ReferenceEquals(current, _projectile))
                    _map.Remove(_entity);
            }
        }
    }
}

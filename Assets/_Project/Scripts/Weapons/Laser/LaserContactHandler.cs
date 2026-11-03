using System;
using Asteroids.Collisions;
using Asteroids.Core;

namespace Asteroids.Weapons.Laser
{
    public sealed class LaserContactHandler : ICollisionResponse
    {
        private readonly ILaserBeamLookup _lookup;

        /// <exception cref="ArgumentNullException"><paramref name="lookup"/> is null.</exception>
        public LaserContactHandler(ILaserBeamLookup lookup) =>
            _lookup = lookup ?? throw new ArgumentNullException(nameof(lookup));

        public CollisionResponseOrder Order => CollisionResponseOrder.LaserContact;

        public void Entered(ICollider first, ICollider second, in Contact contact)
        {
            if (!TryGetEntities(first, second, out var firstEntity, out var secondEntity)) return;

            try
            {
                if (_lookup.TryGet(firstEntity, out var beam)) beam.Entered(secondEntity);
            }
            finally
            {
                if (_lookup.TryGet(secondEntity, out var beam)) beam.Entered(firstEntity);
            }
        }

        public void Stayed(ICollider first, ICollider second, in Contact contact) { }

        public void Exited(ICollider first, ICollider second)
        {
            if (!TryGetEntities(first, second, out var firstEntity, out var secondEntity)) return;

            try
            {
                if (_lookup.TryGet(firstEntity, out var beam)) beam.Exited(secondEntity);
            }
            finally
            {
                if (_lookup.TryGet(secondEntity, out var beam)) beam.Exited(firstEntity);
            }
        }

        private static bool TryGetEntities(ICollider first, ICollider second, out IEntity firstEntity,
            out IEntity secondEntity)
        {
            firstEntity = first?.Owner as IEntity;
            secondEntity = second?.Owner as IEntity;
            return firstEntity != null && secondEntity != null;
        }
    }
}
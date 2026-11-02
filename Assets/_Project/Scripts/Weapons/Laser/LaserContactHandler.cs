using System;
using Asteroids.Collisions;
using Asteroids.Core;

namespace Asteroids.Weapons.Laser
{
    public sealed class LaserContactHandler
    {
        private readonly ILaserBeamLookup _lookup;

        /// <exception cref="ArgumentNullException"><paramref name="lookup"/> is null.</exception>
        public LaserContactHandler(ILaserBeamLookup lookup) =>
            _lookup = lookup ?? throw new ArgumentNullException(nameof(lookup));

        public void OnCollisionEntered(CollisionEntered signal)
        {
            if (TryResolve(signal.Self, signal.Other, out var beam, out var target)) beam.Entered(target);
        }

        public void OnCollisionExited(CollisionExited signal)
        {
            if (TryResolve(signal.Self, signal.Other, out var beam, out var target)) beam.Exited(target);
        }

        private bool TryResolve(ICollider self, ICollider other, out ILaserBeamContacts beam, out IEntity target)
        {
            beam = null;
            target = null;

            if (self?.Owner is not IEntity selfEntity) return false;
            if (other?.Owner is not IEntity otherEntity) return false;
            if (!_lookup.TryGet(selfEntity, out beam)) return false;

            target = otherEntity;
            return true;
        }
    }
}

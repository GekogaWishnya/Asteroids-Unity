using System;
using UnityEngine;
using Zenject;

namespace Asteroids.Collisions
{
    /// <summary>Static invisible wall. Everything beyond the edge of <see cref="Region"/> is penetrating.</summary>
    public sealed class BoundaryCollider : BaseCollider
    {
        public BoundaryCollider(SignalBus signalBus, object owner, HalfPlane region,
            CollisionLayer layer, CollisionLayer mask)
            : base(signalBus, owner, layer, mask)
        {
            if (region.InwardNormal == Vector2.zero)
                throw new ArgumentException("Region is uninitialised (default HalfPlane).", nameof(region));
            Region = region;
        }

        public HalfPlane Region { get; }

        public override bool TryGetContact(ICollider other, out Contact contact)
        {
            switch (other)
            {
                case CircleCollider circle:
                    // Stealing circle against boundary math from CircleCollider and inverting the result.
                    var found = circle.TryGetContact(this, out var circleContact);
                    contact = found ? circleContact.Inverted : default;
                    return found;
                case BoundaryCollider:
                    // Static geometry should never collide with itself.
                    contact = default;
                    return false;
                default:
                    throw new NotSupportedException(
                        $"{nameof(BoundaryCollider)} has no test against {other?.GetType().Name ?? "null"}.");
            }
        }
    }
}
using System;
using UnityEngine;
using Zenject;

namespace Asteroids.Collisions
{
    public sealed class CircleCollider : BaseCollider
    {
        private const float MinDistance = CollisionConstants.MinLength;
        private float _radius;

        public CircleCollider(SignalBus signalBus, object owner, float radius,
            CollisionLayer layer, CollisionLayer mask)
            : base(signalBus, owner, layer, mask)
        {
            Radius = radius;
        }

        /// <summary>World-space center. The owner must update it before <see cref="ICollisionSystem.Step"/>.</summary>
        public Vector2 Position { get; set; }

        public float Radius
        {
            get => _radius;
            set
            {
                if (value < 0f || !float.IsFinite(value))
                    throw new ArgumentOutOfRangeException(nameof(value), "Radius must be finite and non-negative.");
                _radius = value;
            }
        }

        public override bool TryGetContact(ICollider other, out Contact contact)
        {
            return other switch
            {
                CircleCollider circle => ContactWithCircle(circle, out contact),
                BoundaryCollider boundary => ContactWithBoundary(boundary, out contact),
                _ => throw new NotSupportedException(
                    $"{nameof(CircleCollider)} has no test against {other?.GetType().Name ?? "null"}.")
            };
        }

        private bool ContactWithCircle(CircleCollider other, out Contact contact)
        {
            var delta = Position - other.Position;
            var radii = Radius + other.Radius;
            var sqrDistance = delta.sqrMagnitude;

            if (sqrDistance >= radii * radii)
            {
                contact = default;
                return false;
            }

            var distance = Mathf.Sqrt(sqrDistance);
            var normal = distance > MinDistance ? delta / distance : Vector2.up;
            contact = new Contact(normal, radii - distance);
            return true;
        }

        private bool ContactWithBoundary(BoundaryCollider boundary, out Contact contact)
        {
            var depth = Radius - boundary.Region.SignedDistance(Position);
            if (depth <= 0f)
            {
                contact = default;
                return false;
            }

            contact = new Contact(boundary.Region.InwardNormal, depth);
            return true;
        }
    }
}
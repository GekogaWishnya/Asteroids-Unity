using System;
using Asteroids.Core;
using UnityEngine;
using Zenject;

namespace Asteroids.Collisions
{
    public sealed class CircleCollider : BaseCollider
    {
        private const float MinDistance = CollisionConstants.MinLength;
        private readonly IPose _pose;
        private readonly Vector2 _offset;
        private float _radius;

        /// <param name="pose">Source of the collider's position.</param>
        /// <param name="offset">Center offset in the pose's local space (x along <see cref="IPose.Right"/>, y along <see cref="IPose.Up"/>).</param>
        public CircleCollider(SignalBus signalBus, object owner, IPose pose, Vector2 offset, float radius,
            CollisionLayer layer, CollisionLayer mask)
            : base(signalBus, owner, layer, mask)
        {
            _pose = pose ?? throw new ArgumentNullException(nameof(pose));
            _offset = offset;
            Radius = radius;
        }

        /// <summary>World-space center from the current pose.</summary>
        public Vector2 Center => _pose.Position + _pose.Right * _offset.x + _pose.Up * _offset.y;

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
            var delta = Center - other.Center;
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
            var depth = Radius - boundary.Region.SignedDistance(Center);
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

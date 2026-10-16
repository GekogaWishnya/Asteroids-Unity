using UnityEngine;

namespace Asteroids.Collisions
{
    /// <summary>Collision rules of different shape types.</summary>
    public static class ShapeContacts
    {
        private const float MinDistance = CollisionConstants.MinLength;

        public static ShapeContactTable CreateTable() =>
            new ShapeContactTable()
                .Add<CircleShape, CircleShape>(CircleWithCircle)
                .Add<CircleShape, BoundaryShape>(CircleWithBoundary)
                .Add<BoundaryShape, BoundaryShape>(BoundaryWithBoundary);

        private static bool CircleWithCircle(CircleShape first, CircleShape second, out Contact contact)
        {
            var delta = first.Center - second.Center;
            var radii = first.Radius + second.Radius;
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

        private static bool CircleWithBoundary(CircleShape circle, BoundaryShape boundary, out Contact contact)
        {
            var depth = circle.Radius - boundary.Region.SignedDistance(circle.Center);
            if (depth <= 0f)
            {
                contact = default;
                return false;
            }

            contact = new Contact(boundary.Region.InwardNormal, depth);
            return true;
        }

        // Static geometry never collides with itself.
        private static bool BoundaryWithBoundary(BoundaryShape first, BoundaryShape second, out Contact contact)
        {
            contact = default;
            return false;
        }
    }
}
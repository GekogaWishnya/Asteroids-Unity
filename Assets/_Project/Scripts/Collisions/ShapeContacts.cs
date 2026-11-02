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
                .Add<BoundaryShape, BoundaryShape>(BoundaryWithBoundary)
                .Add<OrientedBoxShape, CircleShape>(BoxWithCircle)
                .Add<OrientedBoxShape, BoundaryShape>(BoxWithBoundary)
                .Add<OrientedBoxShape, OrientedBoxShape>(BoxWithBox);

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

        private static bool BoxWithCircle(OrientedBoxShape box, CircleShape circle, out Contact contact)
        {
            var axisX = box.AxisX;
            var axisY = box.AxisY;
            var half = box.HalfExtents;
            var radius = circle.Radius;

            var delta = circle.Center - box.Center;
            var localX = Vector2.Dot(delta, axisX);
            var localY = Vector2.Dot(delta, axisY);

            var outsideX = localX - Mathf.Clamp(localX, -half.x, half.x);
            var outsideY = localY - Mathf.Clamp(localY, -half.y, half.y);

            if (outsideX != 0f || outsideY != 0f)
            {
                var toCircle = axisX * outsideX + axisY * outsideY;
                var sqrDistance = toCircle.sqrMagnitude;
                if (sqrDistance >= radius * radius)
                {
                    contact = default;
                    return false;
                }

                var distance = Mathf.Sqrt(sqrDistance);
                contact = new Contact(-toCircle / distance, radius - distance);
                return true;
            }
            
            var depthX = half.x - Mathf.Abs(localX);
            var depthY = half.y - Mathf.Abs(localY);

            var towardsCircle = depthX < depthY
                ? axisX * (localX < 0f ? -1f : 1f)
                : axisY * (localY < 0f ? -1f : 1f);

            contact = new Contact(-towardsCircle, Mathf.Min(depthX, depthY) + radius);
            return true;
        }

        private static bool BoxWithBoundary(OrientedBoxShape box, BoundaryShape boundary, out Contact contact)
        {
            var region = boundary.Region;
            var center = box.Center;
            var x = box.AxisX * box.HalfExtents.x;
            var y = box.AxisY * box.HalfExtents.y;

            // The deepest corner decides the penetration.
            var nearest = Mathf.Min(
                Mathf.Min(region.SignedDistance(center + x + y), region.SignedDistance(center + x - y)),
                Mathf.Min(region.SignedDistance(center - x + y), region.SignedDistance(center - x - y)));

            if (nearest >= 0f)
            {
                contact = default;
                return false;
            }

            contact = new Contact(region.InwardNormal, -nearest);
            return true;
        }
        
        private static bool BoxWithBox(OrientedBoxShape first, OrientedBoxShape second, out Contact contact)
        {
            var delta = first.Center - second.Center;
            var bestDepth = float.PositiveInfinity;
            var bestNormal = Vector2.up;

            if (!TestAxis(first.AxisX, first, second, delta, ref bestDepth, ref bestNormal) ||
                !TestAxis(first.AxisY, first, second, delta, ref bestDepth, ref bestNormal) ||
                !TestAxis(second.AxisX, first, second, delta, ref bestDepth, ref bestNormal) ||
                !TestAxis(second.AxisY, first, second, delta, ref bestDepth, ref bestNormal))
            {
                contact = default;
                return false;
            }

            contact = new Contact(bestNormal, bestDepth);
            return true;
        }
        
        private static bool TestAxis(
            Vector2 axis,
            OrientedBoxShape first,
            OrientedBoxShape second,
            Vector2 delta,
            ref float bestDepth,
            ref Vector2 bestNormal)
        {
            var distance = Vector2.Dot(delta, axis);
            var depth = ProjectedRadius(first, axis) + ProjectedRadius(second, axis) - Mathf.Abs(distance);
            if (depth <= 0f) return false;

            if (!(depth < bestDepth)) return true;
            
            bestDepth = depth;
            bestNormal = distance < 0f ? -axis : axis;

            return true;
        }

        private static float ProjectedRadius(OrientedBoxShape box, Vector2 axis) =>
            box.HalfExtents.x * Mathf.Abs(Vector2.Dot(box.AxisX, axis)) +
            box.HalfExtents.y * Mathf.Abs(Vector2.Dot(box.AxisY, axis));
    }
}

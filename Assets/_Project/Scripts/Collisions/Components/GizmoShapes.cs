using UnityEngine;

namespace Asteroids.Collisions.Components
{
    internal static class GizmoShapes
    {
        private const int CircleSegments = 48;
        private const float HatchSpacing = 1f;
        private const float HatchLength = 0.4f;
        private const float ArrowLength = 1f;
        private const float ArrowHead = 0.25f;

        public static void Circle(Vector2 center, float radius)
        {
            var previous = center + new Vector2(radius, 0f);
            for (var i = 1; i <= CircleSegments; i++)
            {
                var angle = i * Mathf.PI * 2f / CircleSegments;
                var next = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }

        public static void Box(Vector2 center, Vector2 axisX, Vector2 axisY, Vector2 halfExtents)
        {
            var x = axisX * halfExtents.x;
            var y = axisY * halfExtents.y;
            var a = center + x + y;
            var b = center + x - y;
            var c = center - x - y;
            var d = center - x + y;

            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, d);
            Gizmos.DrawLine(d, a);
        }

        /// <summary>Edge line and an arrow pointing into the allowed area.</summary>
        public static void Wall(Vector2 point, Vector2 inward, float halfLength)
        {
            var tangent = new Vector2(-inward.y, inward.x);
            Gizmos.DrawLine(point - tangent * halfLength, point + tangent * halfLength);

            for (var t = -halfLength; t <= halfLength; t += HatchSpacing)
            {
                var onEdge = point + tangent * t;
                Gizmos.DrawLine(onEdge, onEdge - inward * HatchLength + tangent * (HatchLength * 0.5f));
            }

            var tip = point + inward * ArrowLength;
            Gizmos.DrawLine(point, tip);
            Gizmos.DrawLine(tip, tip - inward * ArrowHead + tangent * ArrowHead);
            Gizmos.DrawLine(tip, tip - inward * ArrowHead - tangent * ArrowHead);
        }
    }
}

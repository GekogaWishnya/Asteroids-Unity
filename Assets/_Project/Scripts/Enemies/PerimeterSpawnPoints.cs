using System;
using Asteroids.Common;
using Asteroids.Physics;
using UnityEngine;

namespace Asteroids.Enemies
{
    /// <summary>
    /// Picks spawn points on the edge of the gameplay area, uniformly over the whole perimeter, and headings
    /// that lead into the area. The far edges are offset in by a bit
    /// so a point stays inside the <see cref="IWorldBounds.Wrap"/>.
    /// Shared by every enemy that enters from the edge. Does NOT avoid the ship.
    /// </summary>
    public sealed class PerimeterSpawnPoints
    {
        /// <summary>Widest an object may turn away from straight into the area in degrees.</summary>
        public const float MaxTiltFromInward = 60f;

        /// <summary>
        /// Distance in world units object is guaranteed to travel before it reaches any edge of the gameplay area.
        /// </summary>
        public const float MinTravel = 2f;

        private const float EdgeInset = 0.001f;

        private readonly IWorldBounds _bounds;
        private readonly IRandom _random;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        /// <exception cref="ArgumentException">The play area is not larger than <see cref="MinTravel"/> in both directions.</exception>
        public PerimeterSpawnPoints(IWorldBounds bounds, IRandom random)
        {
            _bounds = bounds ?? throw new ArgumentNullException(nameof(bounds));
            _random = random ?? throw new ArgumentNullException(nameof(random));

            var rect = bounds.Rect;
            if (rect.width <= MinTravel || rect.height <= MinTravel)
                throw new ArgumentException(
                    $"The play area ({rect.width} x {rect.height}) must be larger than {MinTravel} " +
                    "in both directions for enemies to spawn on its edge.", nameof(bounds));
        }

        /// <summary>A random point of the perimeter.</summary>
        public PerimeterPoint Pick()
        {
            var rect = _bounds.Rect;
            var width = rect.width;
            var height = rect.height;
            var along = _random.Value() * 2f * (width + height);

            if (along < width)
                return new PerimeterPoint(new Vector2(rect.xMin, rect.yMin), Vector2.right, Vector2.up, width, along);
            along -= width;

            if (along < height)
                return new PerimeterPoint(
                    new Vector2(rect.xMax - EdgeInset, rect.yMin), Vector2.up, Vector2.left, height, along);
            along -= height;

            if (along < width)
                return new PerimeterPoint(
                    new Vector2(rect.xMin, rect.yMax - EdgeInset), Vector2.right, Vector2.down, width, along);
            along -= width;

            return new PerimeterPoint(new Vector2(rect.xMin, rect.yMin), Vector2.up, Vector2.right, height, along);
        }

        /// <summary>
        /// A random heading into the area from <paramref name="point"/>, tilted at most
        /// <see cref="MaxTiltFromInward"/> from straight in and never so far toward a corner that it would leave
        /// the area within <see cref="MinTravel"/>.
        /// </summary>
        /// <returns>Degrees, counter-clockwise from +X.</returns>
        public float RollInwardHeading(PerimeterPoint point)
        {
            var towardStart = MaxTilt(point.FromStart);
            var towardEnd = MaxTilt(point.ToEnd);

            var tilt = Mathf.Lerp(-towardStart, towardEnd, _random.Value()) * Mathf.Deg2Rad;
            var direction = point.Inward * Mathf.Cos(tilt) + point.Tangent * Mathf.Sin(tilt);
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }

        private static float MaxTilt(float distanceToCorner) =>
            Mathf.Min(MaxTiltFromInward, Mathf.Asin(Mathf.Min(1f, distanceToCorner / MinTravel)) * Mathf.Rad2Deg);
    }
}

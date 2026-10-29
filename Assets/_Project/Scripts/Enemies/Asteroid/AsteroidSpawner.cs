using System;
using Asteroids.Common;
using Asteroids.Core.IConfigs;
using Asteroids.Physics;
using UnityEngine;

namespace Asteroids.Enemies
{
    /// <summary>
    /// Spawns asteroids of one type on a random point of the edge of the gameplay area. The point is chosen
    /// uniformly over the whole perimeter. The far edges are offset in by a bit so the point stays
    /// inside the half-open rect that <see cref="IWorldBounds.Wrap"/> works with.
    /// Spawn does NOT avoid ship collision.
    /// </summary>
    public sealed class AsteroidSpawner : IEnemySpawner
    {
        /// <summary>Widest a heading may turn away from straight into the area, in degrees.</summary>
        public const float MaxTiltFromInward = 60f;

        /// <summary>
        /// Distance in world units an asteroid is guaranteed to travel before
        /// it reaches any edge of the gameplay area.
        /// </summary>
        public const float MinTravel = 2f;
        
        private const float EdgeInset = 0.001f;

        private readonly AsteroidPopulation _population;
        private readonly IWorldBounds _bounds;
        private readonly IRandom _random;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        /// <exception cref="ArgumentException">The play area is not larger than <see cref="MinTravel"/> in both directions.</exception>
        public AsteroidSpawner(AsteroidPopulation population, IWorldBounds bounds, IRandom random)
        {
            _population = population ?? throw new ArgumentNullException(nameof(population));
            _bounds = bounds ?? throw new ArgumentNullException(nameof(bounds));
            _random = random ?? throw new ArgumentNullException(nameof(random));

            var rect = bounds.Rect;
            if (rect.width <= MinTravel || rect.height <= MinTravel)
                throw new ArgumentException(
                    $"The play area ({rect.width} x {rect.height}) must be larger than {MinTravel} " +
                    "in both directions for asteroids to spawn on its edge.", nameof(bounds));
        }

        public EnemyTypeId Type => _population.Type;
        public int AliveCount => _population.AliveCount;

        public void Spawn()
        {
            var edge = PickEdgePoint();
            _population.Launch(edge.Position, RollHeading(edge));
        }

        /// <summary>Walks the perimeter: bottom, right, top, left.</summary>
        private EdgePoint PickEdgePoint()
        {
            var rect = _bounds.Rect;
            var width = rect.width;
            var height = rect.height;
            var along = _random.Value() * 2f * (width + height);

            if (along < width)
                return new EdgePoint(new Vector2(rect.xMin, rect.yMin), Vector2.right, Vector2.up, width, along);
            along -= width;

            if (along < height)
                return new EdgePoint(
                    new Vector2(rect.xMax - EdgeInset, rect.yMin), Vector2.up, Vector2.left, height, along);
            along -= height;

            if (along < width)
                return new EdgePoint(
                    new Vector2(rect.xMin, rect.yMax - EdgeInset), Vector2.right, Vector2.down, width, along);
            along -= width;

            return new EdgePoint(new Vector2(rect.xMin, rect.yMin), Vector2.up, Vector2.right, height, along);
        }
        
        /// <summary>Heading in degrees.</summary>
        private float RollHeading(EdgePoint edge)
        {
            var towardStart = MaxTilt(edge.FromStart);
            var towardEnd = MaxTilt(edge.ToEnd);

            var tilt = Mathf.Lerp(-towardStart, towardEnd, _random.Value()) * Mathf.Deg2Rad;
            var direction = edge.Inward * Mathf.Cos(tilt) + edge.Tangent * Mathf.Sin(tilt);
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }

        private static float MaxTilt(float distanceToCorner) =>
            Mathf.Min(MaxTiltFromInward, Mathf.Asin(Mathf.Min(1f, distanceToCorner / MinTravel)) * Mathf.Rad2Deg);

        private readonly struct EdgePoint
        {
            public EdgePoint(Vector2 origin, Vector2 tangent, Vector2 inward, float length, float along)
            {
                Position = origin + tangent * along;
                Tangent = tangent;
                Inward = inward;
                FromStart = along;
                ToEnd = Mathf.Max(0f, length - along);
            }

            public Vector2 Position { get; }

            /// <summary>Unit vector along the edge, in the direction the perimeter is walked.</summary>
            public Vector2 Tangent { get; }

            /// <summary>Unit vector pointing into the area.</summary>
            public Vector2 Inward { get; }

            /// <summary>Distance from the point to the start corner of its edge.</summary>
            public float FromStart { get; }

            /// <summary>Distance from the point to the end corner of its edge.</summary>
            public float ToEnd { get; }
        }
    }
}

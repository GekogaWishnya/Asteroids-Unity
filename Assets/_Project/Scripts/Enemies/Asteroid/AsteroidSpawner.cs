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
    internal sealed class AsteroidSpawner : IEnemySpawner
    {
        /// <summary>Widest a heading may turn away from straight into the area, in degrees.</summary>
        public const float MaxTiltFromInward = PerimeterSpawnPoints.MaxTiltFromInward;

        /// <summary>
        /// Distance in world units an asteroid is guaranteed to travel before
        /// it reaches any edge of the gameplay area.
        /// </summary>
        public const float MinTravel = PerimeterSpawnPoints.MinTravel;

        private readonly AsteroidPopulation _population;
        private readonly PerimeterSpawnPoints _perimeter;
        private readonly OffscreenEntry _entry;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        /// <exception cref="ArgumentException">The play area is not larger than <see cref="MinTravel"/> in both directions.</exception>
        public AsteroidSpawner(
            AsteroidPopulation population, IWorldBounds bounds, IRandom random, OffscreenEntry entry)
        {
            _population = population ?? throw new ArgumentNullException(nameof(population));
            _perimeter = new PerimeterSpawnPoints(bounds, random);
            _entry = entry ?? throw new ArgumentNullException(nameof(entry));
        }

        public EnemyTypeId Type => _population.Type;
        public int AliveCount => _population.AliveCount;

        public void DespawnAll() => _population.DespawnAll();

        public void Spawn()
        {
            var point = _perimeter.Pick();
            var heading = _perimeter.RollInwardHeading(point);
            var radians = heading * Mathf.Deg2Rad;
            var direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));

            _population.Launch(_entry.StartFor(point, direction), heading);
        }
    }
}
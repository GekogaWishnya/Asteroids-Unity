using System;
using Asteroids.Common;
using Asteroids.Core.IConfigs;
using Asteroids.Physics;

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
        public const float MaxTiltFromInward = PerimeterSpawnPoints.MaxTiltFromInward;

        /// <summary>
        /// Distance in world units an asteroid is guaranteed to travel before
        /// it reaches any edge of the gameplay area.
        /// </summary>
        public const float MinTravel = PerimeterSpawnPoints.MinTravel;

        private readonly AsteroidPopulation _population;
        private readonly PerimeterSpawnPoints _perimeter;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        /// <exception cref="ArgumentException">The play area is not larger than <see cref="MinTravel"/> in both directions.</exception>
        public AsteroidSpawner(AsteroidPopulation population, IWorldBounds bounds, IRandom random)
        {
            _population = population ?? throw new ArgumentNullException(nameof(population));
            _perimeter = new PerimeterSpawnPoints(bounds, random);
        }

        public EnemyTypeId Type => _population.Type;
        public int AliveCount => _population.AliveCount;

        public void Spawn()
        {
            var point = _perimeter.Pick();
            _population.Launch(point.Position, _perimeter.RollInwardHeading(point));
        }
    }
}

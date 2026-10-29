using System;
using Asteroids.Core;
using UnityEngine;

namespace Asteroids.Enemies
{
    public sealed class AwardScoreOnDeath : IEnemyDeathHandler
    {
        private readonly IScoreSink _score;
        private readonly int _points;

        /// <exception cref="ArgumentNullException"><paramref name="score"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="points"/> is not positive.</exception>
        public AwardScoreOnDeath(IScoreSink score, int points)
        {
            _score = score ?? throw new ArgumentNullException(nameof(score));
            if (points <= 0) throw new ArgumentOutOfRangeException(nameof(points), "Points must be positive.");

            _points = points;
        }

        public void Died(Vector2 position) => _score.Add(_points);
    }
}

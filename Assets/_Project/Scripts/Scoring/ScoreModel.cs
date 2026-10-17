using System;
using Asteroids.Common;
using Asteroids.Core;

namespace Asteroids.Scoring
{
    /// <summary>
    /// The score of the current game. Starts at zero and only grows.
    /// </summary>
    public sealed class ScoreModel : IScore, IScoreSink
    {
        private readonly ObservableValue<int> _value = new(0);

        public IObservableValue<int> Value => _value;

        /// <exception cref="ArgumentOutOfRangeException"><paramref name="points"/> is not positive.</exception>
        public void Add(int points)
        {
            if (points <= 0)
                throw new ArgumentOutOfRangeException(nameof(points), "Points must be positive.");

            var sum = (long)_value.Value + points;
            _value.Value = sum > int.MaxValue ? int.MaxValue : (int)sum;
        }
    }
}

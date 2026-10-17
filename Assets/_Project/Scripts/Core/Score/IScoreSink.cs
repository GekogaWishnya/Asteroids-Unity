using System;

namespace Asteroids.Core
{
    public interface IScoreSink
    {
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="points"/> is not positive.</exception>
        void Add(int points);
    }
}

using System;

namespace Asteroids.Common
{
    /// <summary><see cref="IRandom"/> over <see cref="System.Random"/>. Not thread-safe.</summary>
    public sealed class SystemRandom : IRandom
    {
        /// <summary>The largest float below 1. A double just under 1 can round up to 1.0f when narrowed.</summary>
        private const float BelowOne = 0.99999994f;

        private readonly Random _random;

        /// <summary>Seeded from the environment tick count.</summary>
        public SystemRandom() : this(Environment.TickCount) { }

        public SystemRandom(int seed) => _random = new Random(seed);

        public float Value()
        {
            var value = (float)_random.NextDouble();
            return value < 1f ? value : BelowOne;
        }

        public float Range(float min, float max)
        {
            if (!float.IsFinite(min)) throw new ArgumentException("Min must be finite.", nameof(min));
            if (!float.IsFinite(max)) throw new ArgumentException("Max must be finite.", nameof(max));
            if (min > max) throw new ArgumentException("Min must not be above max.", nameof(min));

            return min + Value() * (max - min);
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), "The interval is empty.");

            return _random.Next(minInclusive, maxExclusive);
        }
    }
}

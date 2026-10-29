namespace Asteroids.Common
{
    /// <summary>Source of randomness for game logic.</summary>
    public interface IRandom
    {
        /// <summary>Uniform value in [0, 1).</summary>
        float Value();

        /// <summary>Uniform value between <paramref name="min"/> and <paramref name="max"/>.</summary>
        /// <exception cref="System.ArgumentException">A bound is not finite, or <paramref name="min"/> is above <paramref name="max"/>.</exception>
        float Range(float min, float max);

        /// <summary>Uniform integer in [<paramref name="minInclusive"/>, <paramref name="maxExclusive"/>).</summary>
        /// <exception cref="System.ArgumentOutOfRangeException">The interval is empty.</exception>
        int Range(int minInclusive, int maxExclusive);
    }
}

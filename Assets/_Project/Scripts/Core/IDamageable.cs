using System;

namespace Asteroids.Core
{
    public interface IDamageable
    {
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="amount"/> is not positive.</exception>
        void TakeDamage(int amount);
    }
}

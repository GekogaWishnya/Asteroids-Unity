using System;

namespace Asteroids.Core
{
    public interface IHealable
    {
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="amount"/> is not positive.</exception>
        void Heal(int amount);
    }
}
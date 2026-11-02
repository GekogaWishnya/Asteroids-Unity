using System;

namespace Asteroids.Core
{
    public interface IWeaponTriggers
    {
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="slot"/> is not a known slot.</exception>
        IFireInput For(WeaponSlot slot);
    }
}

namespace Asteroids.Core.IConfigs
{
    /// <summary>
    /// A laser fires a beam for as long as its trigger is held and its tank has energy. The beam hurts everything
    /// it touches: on enter and every <see cref="HitInterval"/> while the target stays.
    /// </summary>
    public interface ILaserWeaponConfig
    {
        float MaxEnergy { get; }
        float DrainPerSecond { get; }
        float RefillPerSecond { get; }
        float RefillDelay { get; }
        float MinEnergyToFire { get; }
        int HitDamage { get; }
        float HitInterval { get; }

        /// <summary>
        /// Distance in world units from the carrier's position, along its facing, where the beam starts.
        /// </summary>
        float MuzzleOffset { get; }
    }
}

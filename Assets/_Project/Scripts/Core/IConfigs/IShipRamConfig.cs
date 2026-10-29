namespace Asteroids.Core.IConfigs
{
    public interface IShipRamConfig
    {
        /// <summary>Damage the ship deals to whatever it rams.</summary>
        int RamDamage { get; }
    }
}

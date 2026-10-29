namespace Asteroids.Core.IConfigs
{
    public interface IWaveProtectionConfig
    {
        /// <summary>Seconds the ship is protected when a wave starts. 0 turns it off.</summary>
        float WaveStartProtectionSeconds { get; }
    }
}

namespace Asteroids.Core.IConfigs
{
    /// <summary>
    /// Formatting (<see cref="string.Format(string, object)"/>) of the laser's state to show on HUD.
    /// </summary>
    public interface ILaserHudConfig
    {
        /// <summary>{0} is the energy in whole percent, rounded down. For example "LASER {0}%".</summary>
        string EnergyFormat { get; }

        /// <summary>
        /// {0} is the seconds until full, already rounded up to a tenth. For example "FULL IN {0:0.0}s".
        /// </summary>
        string RefillFormat { get; }

        /// <summary>Shown instead of <see cref="RefillFormat"/> when the laser is full.</summary>
        string FullText { get; }
    }
}
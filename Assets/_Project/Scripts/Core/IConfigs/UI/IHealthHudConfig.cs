namespace Asteroids.Core.IConfigs
{
    /// <summary>Formatting (<see cref="string.Format(string, object)"/>) of HP to show on HUD.</summary>
    public interface IHealthHudConfig
    {
        /// <summary>{0} is the current health and {1} the maximum, both ints. For example "HP {0}/{1}".</summary>
        string HealthFormat { get; }
    }
}
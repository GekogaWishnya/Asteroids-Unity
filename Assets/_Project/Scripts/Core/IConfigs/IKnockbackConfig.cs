namespace Asteroids.Core.IConfigs
{
    public interface IKnockbackConfig
    {
        /// <summary>Slowest speed (units/s) the entity leaves a bounce with.</summary>
        float MinKnockbackSpeed { get; }
    }
}

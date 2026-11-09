namespace Asteroids.Core.IConfigs
{
    public interface IFeedbackConfig
    {
        /// <summary>Played when the owner is damaged. Optional.</summary>
        IEffectConfig Hit { get; }

        /// <summary>
        /// Whether <see cref="Hit"/> is also played for the hit that destroys it, unrelated to <see cref="Death"/>.
        /// </summary>
        bool PlayHitOnFatal { get; }

        /// <summary>Played when the owner is destroyed. Optional.</summary>
        IEffectConfig Death { get; }
    }

    public interface IEnemyFeedbackConfig : IFeedbackConfig
    {
        EnemyTypeId Type { get; }
    }
}
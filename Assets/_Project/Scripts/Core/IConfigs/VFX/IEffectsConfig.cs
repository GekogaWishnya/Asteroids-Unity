using System.Collections.Generic;

namespace Asteroids.Core.IConfigs
{
    /// <summary>Which effects are played for which gameplay events.</summary>
    public interface IEffectsConfig
    {
        IFeedbackConfig Ship { get; }
        IReadOnlyList<IEnemyFeedbackConfig> Enemies { get; }
    }
}
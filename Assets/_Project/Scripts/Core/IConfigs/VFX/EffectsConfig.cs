using System;
using System.Collections.Generic;

namespace Asteroids.Core.IConfigs
{
    public sealed class EffectsConfig : IEffectsConfig
    {
        /// <exception cref="ArgumentNullException">An argument is null.</exception>
        public EffectsConfig(IFeedbackConfig ship, IEnumerable<IEnemyFeedbackConfig> enemies)
        {
            Ship = ship ?? throw new ArgumentNullException(nameof(ship));
            if (enemies == null) throw new ArgumentNullException(nameof(enemies));
            Enemies = new List<IEnemyFeedbackConfig>(enemies);
        }

        public IFeedbackConfig Ship { get; }
        public IReadOnlyList<IEnemyFeedbackConfig> Enemies { get; }
    }
}
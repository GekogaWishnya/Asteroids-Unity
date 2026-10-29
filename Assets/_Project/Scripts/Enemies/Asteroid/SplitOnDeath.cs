using System;
using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Enemies
{
    public sealed class SplitOnDeath : IEnemyDeathHandler
    {
        private readonly IAsteroidLauncher _launcher;
        private readonly EnemyTypeId _childType;
        private readonly int _childCount;

        /// <exception cref="ArgumentNullException"><paramref name="launcher"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="childType"/> is not valid.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="childCount"/> is below 1.</exception>
        public SplitOnDeath(IAsteroidLauncher launcher, EnemyTypeId childType, int childCount)
        {
            _launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));
            if (!childType.IsValid) throw new ArgumentException("The child type is not set.", nameof(childType));
            if (childCount < 1)
                throw new ArgumentOutOfRangeException(nameof(childCount), "At least one child is needed.");

            _childType = childType;
            _childCount = childCount;
        }

        public void Died(Vector2 position)
        {
            for (var i = 0; i < _childCount; i++) _launcher.Launch(_childType, position);
        }
    }
}

using System;
using Asteroids.Core;

namespace Asteroids.Enemies
{
    internal sealed class DespawnEnemiesOnRestart : IRestartable
    {
        private readonly IEnemyData _enemies;

        /// <exception cref="ArgumentNullException"><paramref name="enemies"/> is null.</exception>
        public DespawnEnemiesOnRestart(IEnemyData enemies) =>
            _enemies = enemies ?? throw new ArgumentNullException(nameof(enemies));

        public RestartOrder Order => RestartOrder.Enemies;

        public void Restart() => _enemies.DespawnAll();
    }
}
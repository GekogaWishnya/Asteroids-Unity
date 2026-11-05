using System;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Enemies
{
    internal sealed class UfoSpawner : IEnemySpawner
    {
        private const float MinAimDistanceSqr = 1e-6f;

        private readonly IUfoTypeConfig _config;
        private readonly PerimeterSpawnPoints _perimeter;
        private readonly IPlayerTarget _target;
        private readonly OffscreenEntry _entry;
        private readonly EnemyPool<UfoController> _pool;

        /// <param name="createController">Creates one idle UFO, object included.</param>
        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The config's pool max idle is negative.</exception>
        public UfoSpawner(
            IUfoTypeConfig config,
            Func<UfoController> createController,
            PerimeterSpawnPoints perimeter,
            IPlayerTarget target,
            OffscreenEntry entry)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            if (createController == null) throw new ArgumentNullException(nameof(createController));
            _perimeter = perimeter ?? throw new ArgumentNullException(nameof(perimeter));
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _entry = entry ?? throw new ArgumentNullException(nameof(entry));

            _pool = new EnemyPool<UfoController>(createController, config.PoolMaxIdle);
        }

        public EnemyTypeId Type => _config.Type;

        public int AliveCount => _pool.AliveCount;

        public void Prewarm() => _pool.Prewarm(_config.PoolPrewarm);

        public void Spawn()
        {
            var point = _perimeter.Pick();
            var direction = AimFrom(point);
            var start = _entry.StartFor(point, direction);
            _pool.Launch(controller => controller.Launch(start, direction));
        }

        public void DespawnAll() => _pool.DespawnAll();

        private Vector2 AimFrom(PerimeterPoint point)
        {
            if (_target.HasTarget)
            {
                var toTarget = _target.Position - point.Position;
                if (toTarget.sqrMagnitude > MinAimDistanceSqr) return toTarget.normalized;
            }

            var heading = _perimeter.RollInwardHeading(point) * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(heading), Mathf.Sin(heading));
        }
    }
}
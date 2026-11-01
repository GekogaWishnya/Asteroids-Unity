using System;
using Asteroids.Common;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Enemies
{
    public sealed class UfoSpawner : IEnemySpawner
    {
        private const float MinAimDistanceSqr = 1e-6f;

        private readonly IUfoTypeConfig _config;
        private readonly Func<UfoController> _createController;
        private readonly PerimeterSpawnPoints _perimeter;
        private readonly IPlayerTarget _target;
        private readonly ObjectPool<UfoController> _pool;

        /// <param name="createController">Creates one idle UFO, object included.</param>
        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The config's pool max idle is negative.</exception>
        public UfoSpawner(
            IUfoTypeConfig config,
            Func<UfoController> createController,
            PerimeterSpawnPoints perimeter,
            IPlayerTarget target)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _createController = createController ?? throw new ArgumentNullException(nameof(createController));
            _perimeter = perimeter ?? throw new ArgumentNullException(nameof(perimeter));
            _target = target ?? throw new ArgumentNullException(nameof(target));

            _pool = new ObjectPool<UfoController>(
                CreateInstance, onDestroy: DestroyInstance, maxIdle: config.PoolMaxIdle);
        }

        public EnemyTypeId Type => _config.Type;
        
        public int AliveCount { get; private set; }
        
        public void Prewarm() => _pool.Prewarm(_config.PoolPrewarm);

        public void Spawn()
        {
            var point = _perimeter.Pick();
            var direction = AimFrom(point);
            var controller = _pool.Get();

            try
            {
                controller.Launch(point.Position, direction);
            }
            catch
            {
                _pool.Release(controller);
                throw;
            }

            AliveCount++;
        }

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

        private UfoController CreateInstance()
        {
            var controller = _createController();
            controller.Finished += OnFinished;
            return controller;
        }

        private void DestroyInstance(UfoController controller)
        {
            controller.Finished -= OnFinished;
            controller.Dispose();
        }

        private void OnFinished(UfoController controller)
        {
            AliveCount--;
            _pool.Release(controller);
        }
    }
}

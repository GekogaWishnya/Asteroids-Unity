using System;
using System.Collections.Generic;
using Asteroids.Common;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Physics;

namespace Asteroids.Enemies
{
    public sealed class UfoSpawnerBuilder
    {
        private readonly IUfoConfig _config;
        private readonly IEnemyViewFactory _views;
        private readonly IPlayerTarget _target;
        private readonly ISimulationLoop _loop;
        private readonly IDamageRegistry _damageRegistry;
        private readonly IKineticBodyRegistry _bodyRegistry;
        private readonly IScoreSink _score;
        private readonly IWorldBounds _bounds;
        private readonly IRandom _random;

        private IReadOnlyList<IEnemySpawner> _spawners;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public UfoSpawnerBuilder(
            IUfoConfig config,
            IEnemyViewFactory views,
            IPlayerTarget target,
            ISimulationLoop loop,
            IDamageRegistry damageRegistry,
            IKineticBodyRegistry bodyRegistry,
            IScoreSink score,
            IWorldBounds bounds,
            IRandom random)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _views = views ?? throw new ArgumentNullException(nameof(views));
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _loop = loop ?? throw new ArgumentNullException(nameof(loop));
            _damageRegistry = damageRegistry ?? throw new ArgumentNullException(nameof(damageRegistry));
            _bodyRegistry = bodyRegistry ?? throw new ArgumentNullException(nameof(bodyRegistry));
            _score = score ?? throw new ArgumentNullException(nameof(score));
            _bounds = bounds ?? throw new ArgumentNullException(nameof(bounds));
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        /// <exception cref="InvalidOperationException">The UFO config is invalid.</exception>
        public IReadOnlyList<IEnemySpawner> Spawners => _spawners ??= Build();

        private IReadOnlyList<IEnemySpawner> Build()
        {
            ConfigValidation.ThrowIfInvalid("The UFO config", UfoConfigValidator.Validate(_config));

            var perimeter = new PerimeterSpawnPoints(_bounds, _random);
            var spawners = new List<UfoSpawner>();

            foreach (var type in _config.Types)
            {
                var typeConfig = type;
                spawners.Add(new UfoSpawner(typeConfig, () => CreateController(typeConfig), perimeter, _target));
            }

            foreach (var spawner in spawners) spawner.Prewarm();

            return spawners;
        }

        private UfoController CreateController(IUfoTypeConfig type) =>
            new(_views.Create(type.Type),
                type,
                _target,
                _loop,
                _damageRegistry,
                _bodyRegistry,
                CreateDeathHandler(type));

        private IEnemyDeathHandler CreateDeathHandler(IUfoTypeConfig type)
        {
            var handlers = new List<IEnemyDeathHandler>();
            if (type.ScorePoints > 0) handlers.Add(new AwardScoreOnDeath(_score, type.ScorePoints));

            return new CompositeDeathHandler(handlers);
        }
    }
}

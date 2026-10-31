using System;
using System.Collections.Generic;
using Asteroids.Common;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Physics;

namespace Asteroids.Enemies
{
    /// <summary>
    /// Builds one <see cref="AsteroidSpawner"/> per type of the asteroid config, with all populations
    /// connected so that any type can split into any other.
    /// The config is validated first and everything wrong with it is reported in a single exception.
    /// The spawners are built on first access, pools prewarmed, and the same list is returned afterwards.
    /// </summary>
    public sealed class AsteroidSpawnerBuilder
    {
        private readonly IAsteroidConfig _config;
        private readonly IEnemyViewFactory _views;
        private readonly ISimulationLoop _loop;
        private readonly IDamageRegistry _damageRegistry;
        private readonly IKineticBodyRegistry _bodyRegistry;
        private readonly IScoreSink _score;
        private readonly IWorldBounds _bounds;
        private readonly IRandom _random;

        private IReadOnlyList<IEnemySpawner> _spawners;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public AsteroidSpawnerBuilder(
            IAsteroidConfig config,
            IEnemyViewFactory views,
            ISimulationLoop loop,
            IDamageRegistry damageRegistry,
            IKineticBodyRegistry bodyRegistry,
            IScoreSink score,
            IWorldBounds bounds,
            IRandom random)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _views = views ?? throw new ArgumentNullException(nameof(views));
            _loop = loop ?? throw new ArgumentNullException(nameof(loop));
            _damageRegistry = damageRegistry ?? throw new ArgumentNullException(nameof(damageRegistry));
            _bodyRegistry = bodyRegistry ?? throw new ArgumentNullException(nameof(bodyRegistry));
            _score = score ?? throw new ArgumentNullException(nameof(score));
            _bounds = bounds ?? throw new ArgumentNullException(nameof(bounds));
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        /// <exception cref="InvalidOperationException">The asteroid config is invalid.</exception>
        public IReadOnlyList<IEnemySpawner> Spawners => _spawners ??= Build();

        private IReadOnlyList<IEnemySpawner> Build()
        {
            ConfigValidation.ThrowIfInvalid("The asteroid config", AsteroidConfigValidator.Validate(_config));

            var launcher = new AsteroidLauncher();
            var populations = new List<AsteroidPopulation>();
            var spawners = new List<IEnemySpawner>();

            foreach (var type in _config.Types)
            {
                var typeConfig = type;
                var population = new AsteroidPopulation(
                    typeConfig, () => CreateController(typeConfig, launcher), _random);

                launcher.Register(population);
                populations.Add(population);
                spawners.Add(new AsteroidSpawner(population, _bounds, _random));
            }

            foreach (var population in populations) population.Prewarm();

            return spawners;
        }

        private AsteroidController CreateController(IAsteroidTypeConfig type, IAsteroidLauncher launcher) =>
            new(_views.Create(type.Type),
                type,
                _loop,
                _damageRegistry,
                _bodyRegistry,
                CreateDeathHandler(type, launcher));

        private IEnemyDeathHandler CreateDeathHandler(IAsteroidTypeConfig type, IAsteroidLauncher launcher)
        {
            var handlers = new List<IEnemyDeathHandler>();
            if (type.ScorePoints > 0) handlers.Add(new AwardScoreOnDeath(_score, type.ScorePoints));
            if (type.ChildCount > 0) handlers.Add(new SplitOnDeath(launcher, type.ChildType, type.ChildCount));

            return new CompositeDeathHandler(handlers);
        }
    }
}

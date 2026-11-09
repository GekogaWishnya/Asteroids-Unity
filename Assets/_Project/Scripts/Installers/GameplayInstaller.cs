using System;
using System.Collections.Generic;
using System.Linq;
using Asteroids.Collisions;
using Asteroids.Common;
using Asteroids.Controls;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Damage;
using Asteroids.Data;
using Asteroids.VFX;
using Asteroids.VFX.Components;
using Asteroids.Enemies;
using Asteroids.Enemies.Components;
using Asteroids.Player;
using Asteroids.Scoring;
using Asteroids.Session;
using Asteroids.Simulation;
using Asteroids.UI;
using Asteroids.Waves;
using Asteroids.Weapons;
using Asteroids.Weapons.Components;
using Asteroids.World;
using UnityEngine;
using Zenject;

namespace Asteroids.Installers
{
    public sealed class GameplayInstaller : MonoInstaller
    {
        [SerializeField] private ShipMovementConfig shipMovement;
        [SerializeField] private ShipHealthConfig shipHealth;
        [SerializeField] private WorldConfig worldConfig;
        [SerializeField] private HudConfig hudConfig;
        [SerializeField] private AsteroidConfig asteroidConfig;
        [SerializeField] private UfoConfig ufoConfig;
        [SerializeField] private LevelConfig levelConfig;
        [SerializeField] private ShipLoadoutDefinition shipLoadout;
        [SerializeField] private RunEndConfig runEndConfig;

        public override void InstallBindings()
        {
            Require(shipMovement, nameof(shipMovement));
            Require(shipHealth, nameof(shipHealth));
            Require(worldConfig, nameof(worldConfig));
            Require(hudConfig, nameof(hudConfig));
            Require(levelConfig, nameof(levelConfig));
            Require(runEndConfig, nameof(runEndConfig));

            SimulationInstaller.Install(Container);
            ControlsInstaller.Install(Container);
            CollisionInstaller.Install(Container);
            DamageInstaller.Install(Container);
            WorldInstaller.Install(Container);
            PlayerInstaller.Install(Container);
            ScoringInstaller.Install(Container);
            UIInstaller.Install(Container);
            EnemiesInstaller.Install(Container);
            WavesInstaller.Install(Container);
            WeaponsInstaller.Install(Container);
            SessionInstaller.Install(Container);
            EffectsInstaller.Install(Container);

            Container.Bind<ISimulationTime>().To<UnityTimeSource>().AsSingle();
            Container.Bind<IShipMovementConfig>().FromInstance(shipMovement);
            Container.Bind<IShipHealthConfig>().FromInstance(shipHealth);
            Container.Bind<IShipRamConfig>().FromInstance(shipHealth);
            Container.Bind<IInvulnerabilityConfig>().FromInstance(shipHealth);
            Container.Bind<IWaveProtectionConfig>().FromInstance(shipHealth);
            Container.Bind<IControlLossConfig>().FromInstance(shipHealth);
            Container.Bind<IWorldLayout>().FromInstance(worldConfig);
            Container.Bind<IWorldAppearance>().FromInstance(worldConfig);
            Container.Bind<ISpawnEntryConfig>().FromInstance(worldConfig);
            Container.Bind<IHudLayout>().FromInstance(hudConfig);
            Container.Bind<IScoreHudConfig>().FromInstance(hudConfig);
            Container.Bind<IHealthHudConfig>().FromInstance(hudConfig);
            Container.Bind<ILaserHudConfig>().FromInstance(hudConfig);
            Container.Bind<ITelemetryHudConfig>().FromInstance(hudConfig);
            Container.Bind<IAsteroidConfig>().FromInstance(asteroidConfig);
            Container.Bind<IUfoConfig>().FromInstance(ufoConfig);
            Container.Bind<ILevelConfig>().FromInstance(levelConfig);
            Container.Bind<IRunEndConfig>().FromInstance(runEndConfig);
            Container.Bind<IRunEndScreenConfig>().FromInstance(runEndConfig);

            Container.Bind<IRandom>().FromInstance(new SystemRandom());
            ShipLoadoutInstaller.Install(Container, shipLoadout);

            var enemyTypes = EnemyTypes.Collect(EnemyTypeSources());
            var enemyPrefabs = enemyTypes.Select(type =>
                new KeyValuePair<EnemyTypeId, GameObject>(type.Id, type.Prefab));
            Container.Bind<EnemyPrefabCatalog>().FromInstance(new EnemyPrefabCatalog(enemyPrefabs));
            Container.Bind<IEnemyViewFactory>().To<PrefabEnemyViewFactory>().AsSingle();

            Container.Bind<IEffectsConfig>().FromInstance(new EffectsConfig(shipHealth.Effects, enemyTypes));
            Container.Bind<IEffectFactory>().To<PrefabEffectFactory>().AsSingle();
        }

        private List<IEnemyTypeSource> EnemyTypeSources()
        {
            var sources = new List<IEnemyTypeSource>();
            if (asteroidConfig) sources.Add(asteroidConfig);
            if (ufoConfig) sources.Add(ufoConfig);
            return sources;
        }

        /// <exception cref="InvalidOperationException">The field is not assigned.</exception>
        private void Require(UnityEngine.Object value, string field)
        {
            if (!value)
                throw new InvalidOperationException(
                    $"{nameof(GameplayInstaller)} on '{name}': '{field}' is not assigned.");
        }
    }
}
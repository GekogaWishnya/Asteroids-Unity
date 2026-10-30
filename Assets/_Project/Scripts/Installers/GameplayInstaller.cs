using System;
using System.Collections.Generic;
using Asteroids.Collisions;
using Asteroids.Common;
using Asteroids.Controls;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Damage;
using Asteroids.Data;
using Asteroids.Enemies;
using Asteroids.Enemies.Components;
using Asteroids.Player;
using Asteroids.Scoring;
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
        [SerializeField] private LevelConfig levelConfig;
        [SerializeField] private EnemyTypeDefinition[] enemyTypes;
        [SerializeField] private WeaponDefinition shipWeapon;

        public override void InstallBindings()
        {
            SimulationInstaller.Install(Container);
            CollisionInstaller.Install(Container);
            DamageInstaller.Install(Container);
            WorldInstaller.Install(Container);
            PlayerInstaller.Install(Container);
            ScoringInstaller.Install(Container);
            UIInstaller.Install(Container);
            EnemiesInstaller.Install(Container);
            WavesInstaller.Install(Container);
            WeaponsInstaller.Install(Container);

            Container.Bind<ISimulationTime>().To<UnityTimeSource>().AsSingle();
            Container.BindInterfacesTo<ShipInput>().AsSingle();
            Container.Bind<IShipMovementConfig>().FromInstance(shipMovement);
            Container.Bind<IShipHealthConfig>().FromInstance(shipHealth);
            Container.Bind<IShipRamConfig>().FromInstance(shipHealth);
            Container.Bind<IInvulnerabilityConfig>().FromInstance(shipHealth);
            Container.Bind<IWaveProtectionConfig>().FromInstance(shipHealth);
            Container.Bind<IWorldLayout>().FromInstance(worldConfig);
            Container.Bind<IWorldAppearance>().FromInstance(worldConfig);
            Container.Bind<IHudLayout>().FromInstance(hudConfig);
            Container.Bind<IAsteroidConfig>().FromInstance(asteroidConfig);
            Container.Bind<ILevelConfig>().FromInstance(levelConfig);

            Container.Bind<IRandom>().FromInstance(new SystemRandom());
            Container.Bind<EnemyPrefabCatalog>().FromInstance(CreatePrefabCatalog());
            Container.Bind<IEnemyViewFactory>().To<PrefabEnemyViewFactory>().AsSingle();

            Container.Bind<IWeapon>()
                .FromMethod(context => CreateShipWeapon(context.Container))
                .AsSingle()
                .WhenInjectedInto<ShipController>();
        }

        private IWeapon CreateShipWeapon(DiContainer container)
        {
            if (!shipWeapon)
                throw new InvalidOperationException("The gameplay installer has no ship weapon assigned.");

            return shipWeapon.Create(container);
        }

        private EnemyPrefabCatalog CreatePrefabCatalog()
        {
            var prefabs = new List<KeyValuePair<EnemyTypeId, GameObject>>();
            var seen = new HashSet<EnemyTypeId>();

            foreach (var type in enemyTypes ?? Array.Empty<EnemyTypeDefinition>())
            {
                if (!type)
                    throw new InvalidOperationException(
                        "The list of enemy types on the gameplay installer has an empty entry.");
                if (!type.Id.IsValid)
                    throw new InvalidOperationException($"Enemy type '{type.name}' has no name.");
                if (!seen.Add(type.Id))
                    throw new InvalidOperationException(
                        $"Two enemy types are named '{type.name}'. Names must be unique.");

                prefabs.Add(new KeyValuePair<EnemyTypeId, GameObject>(type.Id, type.Prefab));
            }

            return new EnemyPrefabCatalog(prefabs);
        }
    }
}

using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Controls;
using Asteroids.Collisions;
using Asteroids.Data;
using Asteroids.Player;
using Asteroids.Simulation;
using Asteroids.World;
using UnityEngine;
using Zenject;

namespace Asteroids.Installers
{
    public sealed class GameplayInstaller : MonoInstaller
    {
        [SerializeField] private ShipMovementConfig shipMovement;
        [SerializeField] private WorldConfig worldConfig;

        public override void InstallBindings()
        {
            SimulationInstaller.Install(Container);
            CollisionInstaller.Install(Container);
            WorldInstaller.Install(Container);
            PlayerInstaller.Install(Container);

            Container.Bind<ISimulationTime>().To<UnityTimeSource>().AsSingle();
            Container.BindInterfacesTo<ShipInput>().AsSingle();
            Container.Bind<IShipMovementConfig>().FromInstance(shipMovement);
            Container.Bind<IWorldLayout>().FromInstance(worldConfig);
            Container.Bind<IWorldAppearance>().FromInstance(worldConfig);
        }
    }
}

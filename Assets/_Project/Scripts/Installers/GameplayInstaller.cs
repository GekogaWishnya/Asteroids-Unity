using Asteroids.Core;
using Asteroids.Data;
using Asteroids.Controls;
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
            Container.BindInterfacesTo<ShipInput>().AsSingle();
            Container.Bind<IShipMovementConfig>().FromInstance(shipMovement);
            Container.Bind<IWorldLayout>().FromInstance(worldConfig);
            Container.Bind<IWorldAppearance>().FromInstance(worldConfig);
            Container.Bind<IWorldBounds>().To<WorldBounds>().AsSingle();
            Container.Bind<IBoundaryPolicy>().To<WrapBoundaryPolicy>().AsSingle();
        }
    }
}
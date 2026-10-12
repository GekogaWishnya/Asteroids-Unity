using Asteroids.Core;
using Asteroids.Data;
using Asteroids.Input;
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
            Container.Bind<WorldConfig>().FromInstance(worldConfig);
        }
    }
}
using Asteroids.Player;
using Zenject;

namespace Asteroids.Installers
{
    public class GameplayInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            InstallSignals();
            InstallServices();
        }
        
        private void InstallSignals()
        {
            
        }

        private void InstallServices()
        {
            Container.BindInterfacesAndSelfTo<ShipInput>().AsSingle();
        }
    }
}
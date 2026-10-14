using Zenject;

namespace Asteroids.Installers
{
    public class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);
            InstallSignals();
        }
        
        private void InstallSignals()
        {
            
        }
    }
}

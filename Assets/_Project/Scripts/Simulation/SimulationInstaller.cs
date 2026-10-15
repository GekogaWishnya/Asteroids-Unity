using Zenject;

namespace Asteroids.Simulation
{
    public sealed class SimulationInstaller : Installer<SimulationInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<SimulationLoop>().AsSingle();
        }
    }
}
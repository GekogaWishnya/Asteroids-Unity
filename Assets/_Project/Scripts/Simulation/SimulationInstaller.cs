using Asteroids.Common;
using Zenject;

namespace Asteroids.Simulation
{
    public sealed class SimulationInstaller : Installer<SimulationInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IErrorLog>().To<UnityErrorLog>().AsSingle();
            Container.BindInterfacesTo<SimulationLoop>().AsSingle();
        }
    }
}

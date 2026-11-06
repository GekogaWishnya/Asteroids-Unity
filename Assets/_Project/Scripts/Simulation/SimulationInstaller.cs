using Asteroids.Common;
using Asteroids.Core;
using Zenject;

namespace Asteroids.Simulation
{
    public sealed class SimulationInstaller : Installer<SimulationInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IErrorLog>().To<UnityErrorLog>().AsSingle();

            Container.Bind<SimulationLoop>().AsSingle();
            Container.Bind(typeof(ISimulationLoop), typeof(ITickable)).To<SimulationLoop>().FromResolve();
        }
    }
}
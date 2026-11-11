using Asteroids.Common;
using Asteroids.Core;
using Zenject;

namespace Asteroids.Simulation
{
    /// <summary>Binds the <see cref="SimulationLoop"/>.</summary>
    public sealed class SimulationInstaller : Installer<SimulationInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IErrorLog>().To<UnityErrorLog>().AsSingle();

            Container.Bind<SimulationLoop>().AsSingle();
            Container.Bind<ISimulationLoop>().To<SimulationLoop>().FromResolve();
        }
    }
}
using Asteroids.Core;
using Asteroids.Simulation;
using Zenject;

namespace Asteroids.Session
{
    /// <summary>
    /// Requires <see cref="SimulationInstaller"/> to be installed. Binds <see cref="IGameSession"/>, which collects
    /// every <see cref="IRestartable"/> bound by the other installers. The session is created at startup.
    /// </summary>
    public sealed class SessionInstaller : Installer<SessionInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<ISimulationControl>().To<SimulationLoop>().FromResolve().WhenInjectedInto<GameSession>();
            Container.Bind<IGameSession>().To<GameSession>().AsSingle().NonLazy();
        }
    }
}
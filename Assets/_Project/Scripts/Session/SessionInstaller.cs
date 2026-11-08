using Asteroids.Core;
using Asteroids.Simulation;
using Zenject;

namespace Asteroids.Session
{
    /// <summary>
    /// Requires <see cref="SimulationInstaller"/> to be installed, <see cref="SignalBus"/> to be installed,
    /// <see cref="Asteroids.Core.IConfigs.IRunEndConfig"/>, <see cref="IScore"/> and <see cref="IWaveProgress"/>
    /// to be bound, and <see cref="ShipDestroyed"/> and <see cref="LevelCompleted"/> to be declared by the
    /// installers that fire them.
    /// Declares <see cref="RunEnded"/> and binds <see cref="IGameSession"/>, which collects every
    /// <see cref="IRestartable"/> bound by the other installers. The session is created at startup.
    /// </summary>
    public sealed class SessionInstaller : Installer<SessionInstaller>
    {
        public override void InstallBindings()
        {
            Container.DeclareSignal<RunEnded>().OptionalSubscriber();
            Container.Bind<IRunEventSink>().To<SignalBusRunEventSink>().AsSingle();

            Container.Bind<RunLifecycle>().AsSingle();
            Container.Bind<IRestartable>().To<RunLifecycle>().FromResolve();
            Container.BindInterfacesTo<RunLifecycleRegistration>().AsSingle();

            Container.Bind<ISimulationControl>().To<SimulationLoop>().FromResolve().WhenInjectedInto<GameSession>();
            Container.BindInterfacesTo<GameSession>().AsSingle().NonLazy();

            Container.BindSignal<ShipDestroyed>()
                .ToMethod<IGameSession>(session => () => session.Finish(RunOutcome.Defeat))
                .FromResolve();
            Container.BindSignal<LevelCompleted>()
                .ToMethod<IGameSession>(session => () => session.Finish(RunOutcome.Victory))
                .FromResolve();
        }
    }
}
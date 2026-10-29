using Asteroids.Core;
using Zenject;

namespace Asteroids.Waves
{
    /// <summary>
    /// Requires <see cref="Asteroids.Core.IConfigs.ILevelConfig"/> to be bound by the host installer,
    /// <see cref="IEnemyData"/> and <see cref="ISimulationLoop"/> to be bound, and <see cref="SignalBus"/>
    /// to be installed. Declares <see cref="WaveStarted"/>, <see cref="WaveCleared"/> and
    /// <see cref="LevelCompleted"/>, which live in <c>Core</c> so other features can react to them.
    /// </summary>
    public sealed class WavesInstaller : Installer<WavesInstaller>
    {
        public override void InstallBindings()
        {
            Container.DeclareSignal<WaveStarted>().OptionalSubscriber();
            Container.DeclareSignal<WaveCleared>().OptionalSubscriber();
            Container.DeclareSignal<LevelCompleted>().OptionalSubscriber();

            Container.Bind<IWaveEventSink>().To<SignalBusWaveEventSink>().AsSingle();
            Container.Bind<WaveDirector>().AsSingle();
            Container.BindInterfacesTo<WaveDirectorRegistration>().AsSingle();
        }
    }
}

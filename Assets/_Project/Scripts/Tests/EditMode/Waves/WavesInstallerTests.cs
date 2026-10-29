using Asteroids.Core;
using Asteroids.Core.IConfigs;
using NUnit.Framework;
using Zenject;
using static Asteroids.Waves.Tests.Build;

namespace Asteroids.Waves.Tests
{
    /// <summary>Checks that the bindings and signal declarations of <see cref="WavesInstaller"/> work together.</summary>
    [TestFixture]
    public sealed class WavesInstallerTests
    {
        private sealed class Rig
        {
            public readonly DiContainer Container = new();
            public readonly FakeEnemyData Data = new("A");

            public Rig(TestLevel level)
            {
                SignalBusInstaller.Install(Container);
                Container.Bind<ILevelConfig>().FromInstance(level);
                Container.Bind<IEnemyData>().FromInstance(Data);
                Container.Bind<ISimulationLoop>().FromInstance(new FakeSimulationLoop());

                WavesInstaller.Install(Container);
            }
        }

        [Test]
        public void Resolve_Director_IsASingleton()
        {
            var rig = new Rig(Level(Wave(0f, Entry("A"))));

            Assert.That(rig.Container.Resolve<WaveDirector>(), Is.SameAs(rig.Container.Resolve<WaveDirector>()));
        }

        [Test]
        public void Tick_FiresTheWaveSignals()
        {
            var rig = new Rig(Level(Wave(0f, Entry("A"))));
            rig.Data.SpawnAddsToAlive = false;
            var bus = rig.Container.Resolve<SignalBus>();
            WaveStarted? started = null;
            WaveCleared? cleared = null;
            LevelCompleted? completed = null;
            bus.Subscribe<WaveStarted>(signal => started = signal);
            bus.Subscribe<WaveCleared>(signal => cleared = signal);
            bus.Subscribe<LevelCompleted>(signal => completed = signal);

            rig.Container.Resolve<WaveDirector>().Tick(0.1f);

            Assert.That(started.HasValue && started.Value.Number == 1 && started.Value.Total == 1, Is.True);
            Assert.That(cleared.HasValue && cleared.Value.Number == 1, Is.True);
            Assert.That(completed.HasValue && completed.Value.WaveCount == 1, Is.True);
        }

        [Test]
        public void Resolve_Director_InvalidLevel_FailsAtOnce()
        {
            var rig = new Rig(Level(Wave(0f, Entry("Ghost"))));

            var ex = Assert.Catch<System.Exception>(() => rig.Container.Resolve<WaveDirector>());

            // Zenject may wrap the exception, so the whole chain is searched.
            StringAssert.Contains("Ghost", ex.ToString());
        }
    }
}

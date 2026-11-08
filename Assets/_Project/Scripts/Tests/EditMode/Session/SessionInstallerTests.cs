using System;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Simulation;
using NUnit.Framework;
using Zenject;

namespace Asteroids.Session.Tests
{
    /// <summary>
    /// Checks the bindings of <see cref="SessionInstaller"/> together with <see cref="SimulationInstaller"/>. The rig
    /// stands in for the other installers: it installs SignalBus, declares the signals Player and Waves own, and
    /// binds the config, score and wave progress the session reads.
    /// </summary>
    [TestFixture]
    public sealed class SessionInstallerTests
    {
        private sealed class FakeTime : ISimulationTime
        {
            public float DeltaTime => 0.02f;
        }

        private sealed class CountingRestartable : IRestartable
        {
            public CountingRestartable(RestartOrder order) => Order = order;

            public RestartOrder Order { get; }
            public int Restarts { get; private set; }

            public void Restart() => Restarts++;
        }

        private static DiContainer Install(params IRestartable[] restartables) =>
            Install(new FakeRunEndConfig(), restartables);

        private static DiContainer Install(FakeRunEndConfig config, params IRestartable[] restartables)
        {
            var container = new DiContainer();
            SignalBusInstaller.Install(container);
            container.DeclareSignal<ShipDestroyed>().OptionalSubscriber();
            container.DeclareSignal<LevelCompleted>().OptionalSubscriber();

            container.Bind<ISimulationTime>().FromInstance(new FakeTime());
            container.Bind<IRunEndConfig>().FromInstance(config);
            container.Bind<IScore>().FromInstance(new FakeScore());
            container.Bind<IWaveProgress>().FromInstance(new FakeWaveProgress());
            SimulationInstaller.Install(container);
            foreach (var restartable in restartables) container.Bind<IRestartable>().FromInstance(restartable);
            SessionInstaller.Install(container);
            return container;
        }

        /// <summary>
        /// Starts the container as a scene context would: resolving the roots creates the signal subscriptions,
        /// and initializing registers the simulation steps.
        /// </summary>
        private static DiContainer Started(DiContainer container)
        {
            container.ResolveRoots();
            foreach (var initializable in container.ResolveAll<IInitializable>()) initializable.Initialize();
            return container;
        }

        [Test]
        public void Pause_PausesTheSimulationLoop()
        {
            var container = Install();

            container.Resolve<IGameSession>().Pause();

            Assert.That(container.Resolve<SimulationLoop>().IsPaused, Is.True);
        }

        [Test]
        public void SimulationControl_IsNotAvailableOutsideTheSession()
        {
            var container = Install();

            Assert.That(container.TryResolve<ISimulationControl>(), Is.Null);
        }

        [Test]
        public void Restart_RestartsEveryBoundRestartable()
        {
            var enemies = new CountingRestartable(RestartOrder.Enemies);
            var score = new CountingRestartable(RestartOrder.Score);
            var container = Install(enemies, score);

            container.Resolve<IGameSession>().Restart();

            Assert.That(enemies.Restarts, Is.EqualTo(1));
            Assert.That(score.Restarts, Is.EqualTo(1));
        }

        [Test]
        public void ResolveRoots_TwoRestartablesWithTheSameOrder_FailsAtStartup()
        {
            var container = Install(
                new CountingRestartable(RestartOrder.Score), new CountingRestartable(RestartOrder.Score));

            Assert.Catch<Exception>(() => container.ResolveRoots());
        }

        [Test]
        public void ResolveRoots_InvalidRunEndConfig_FailsAtStartup()
        {
            var container = Install(new FakeRunEndConfig { VictoryDelay = float.NaN });

            Assert.Catch<Exception>(() => container.ResolveRoots());
        }

        [Test]
        public void ShipDestroyed_FinishesTheRunAsADefeat()
        {
            var container = Started(Install());

            container.Resolve<SignalBus>().Fire(new ShipDestroyed(null));

            var state = container.Resolve<IGameSession>().State.Value;
            Assert.That(state.Phase, Is.EqualTo(RunPhase.Ending));
            Assert.That(state.Result.Outcome, Is.EqualTo(RunOutcome.Defeat));
        }

        [Test]
        public void LevelCompleted_FinishesTheRunAsAVictory()
        {
            var container = Started(Install());

            container.Resolve<SignalBus>().Fire(new LevelCompleted(5));

            Assert.That(container.Resolve<IGameSession>().State.Value.Result.Outcome, Is.EqualTo(RunOutcome.Victory));
        }

        /// <summary>
        /// The whole chain through the real loop: the lifecycle is registered as a step, counts the delay in
        /// simulated time, and the session pauses the loop and fires <see cref="RunEnded"/>.
        /// </summary>
        [Test]
        public void RunEnded_FiresAndPausesOnceTheDelayHasRunInTheLoop()
        {
            var container = Started(Install(new FakeRunEndConfig { DefeatDelay = 0.05f }));
            var bus = container.Resolve<SignalBus>();
            var loop = container.Resolve<SimulationLoop>();
            RunEnded? ended = null;
            bus.Subscribe<RunEnded>(signal => ended = signal);

            bus.Fire(new ShipDestroyed(null));
            loop.Tick();
            loop.Tick();
            Assert.That(ended.HasValue, Is.False);

            loop.Tick();

            Assert.That(ended.HasValue, Is.True);
            Assert.That(ended.Value.Result.Outcome, Is.EqualTo(RunOutcome.Defeat));
            Assert.That(loop.IsPaused, Is.True);
        }

        [Test]
        public void Restart_IncludesTheRunLifecycle()
        {
            var container = Started(Install(new FakeRunEndConfig { DefeatDelay = 0f }));
            var session = container.Resolve<IGameSession>();
            container.Resolve<SignalBus>().Fire(new ShipDestroyed(null));
            Assert.That(session.State.Value.Phase, Is.EqualTo(RunPhase.Ended));

            session.Restart();

            Assert.That(session.State.Value, Is.EqualTo(RunState.Playing));
            Assert.That(session.IsPaused, Is.False);
        }
    }
}

using System;
using Asteroids.Core;
using Asteroids.Simulation;
using NUnit.Framework;
using Zenject;

namespace Asteroids.Session.Tests
{
    /// <summary>Checks the bindings of <see cref="SessionInstaller"/> together with <see cref="SimulationInstaller"/>.</summary>
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

        private static DiContainer Install(params IRestartable[] restartables)
        {
            var container = new DiContainer();
            container.Bind<ISimulationTime>().FromInstance(new FakeTime());
            SimulationInstaller.Install(container);
            foreach (var restartable in restartables) container.Bind<IRestartable>().FromInstance(restartable);
            SessionInstaller.Install(container);
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
    }
}

using System;
using System.Collections.Generic;
using Asteroids.Core;
using NUnit.Framework;

namespace Asteroids.Session.Tests
{
    [TestFixture]
    public sealed class GameSessionTests
    {
        private sealed class FakeSimulation : ISimulationControl
        {
            public bool IsPaused { get; set; }
        }

        private sealed class FakeRestartable : IRestartable
        {
            private readonly List<string> _log;

            public FakeRestartable(RestartOrder order, List<string> log)
            {
                Order = order;
                _log = log;
            }

            public RestartOrder Order { get; }
            public Exception Failure { get; set; }

            public void Restart()
            {
                _log.Add(Order.ToString());
                if (Failure != null) throw Failure;
            }
        }

        private FakeSimulation _simulation;
        private List<string> _log;

        [SetUp]
        public void SetUp()
        {
            _simulation = new FakeSimulation();
            _log = new List<string>();
        }

        private FakeRestartable Restartable(RestartOrder order) => new(order, _log);

        [Test]
        public void PauseAndResume_SwitchTheSimulation()
        {
            var session = new GameSession(_simulation, Array.Empty<IRestartable>());

            session.Pause();
            Assert.That(_simulation.IsPaused, Is.True);
            Assert.That(session.IsPaused, Is.True);

            session.Resume();
            Assert.That(_simulation.IsPaused, Is.False);
            Assert.That(session.IsPaused, Is.False);
        }

        [Test]
        public void Restart_RestartsEverythingInRestartOrder()
        {
            var session = new GameSession(_simulation, new IRestartable[]
            {
                Restartable(RestartOrder.Score),
                Restartable(RestartOrder.Enemies),
                Restartable(RestartOrder.Waves),
                Restartable(RestartOrder.Ship)
            });

            session.Restart();

            Assert.That(_log, Is.EqualTo(new[] { "Enemies", "Ship", "Waves", "Score" }));
        }

        [Test]
        public void Restart_WhilePaused_Resumes()
        {
            var session = new GameSession(_simulation, Array.Empty<IRestartable>());
            session.Pause();

            session.Restart();

            Assert.That(_simulation.IsPaused, Is.False);
        }

        [Test]
        public void Restart_OneFails_RestartsTheRestResumesAndRethrowsTheFirst()
        {
            var first = new InvalidOperationException("first");
            var enemies = Restartable(RestartOrder.Enemies);
            enemies.Failure = first;
            var ship = Restartable(RestartOrder.Ship);
            ship.Failure = new InvalidOperationException("second");
            var session = new GameSession(_simulation, new IRestartable[] { enemies, ship, Restartable(RestartOrder.Score) });
            session.Pause();

            var thrown = Assert.Throws<InvalidOperationException>(() => session.Restart());

            Assert.That(thrown, Is.SameAs(first));
            Assert.That(_log, Is.EqualTo(new[] { "Enemies", "Ship", "Score" }));
            Assert.That(_simulation.IsPaused, Is.False);
        }

        [Test]
        public void Constructor_TwoWithTheSameOrder_Throws()
        {
            var restartables = new IRestartable[] { Restartable(RestartOrder.Ship), Restartable(RestartOrder.Ship) };

            var error = Assert.Throws<ArgumentException>(() => new GameSession(_simulation, restartables));

            StringAssert.Contains("Ship", error.Message);
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new GameSession(null, Array.Empty<IRestartable>()));
            Assert.Throws<ArgumentNullException>(() => new GameSession(_simulation, null));
            Assert.Throws<ArgumentException>(() => new GameSession(_simulation, new IRestartable[] { null }));
        }

        [Test]
        public void RestartOrder_PutsEnemiesBeforeTheShipAndTheShipBeforeTheWaves()
        {
            Assert.That(RestartOrder.Enemies, Is.LessThan(RestartOrder.Ship));
            Assert.That(RestartOrder.Ship, Is.LessThan(RestartOrder.Waves));
        }
    }
}

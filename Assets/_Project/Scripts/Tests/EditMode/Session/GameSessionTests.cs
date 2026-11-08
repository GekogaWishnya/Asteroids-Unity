using System;
using System.Collections.Generic;
using Asteroids.Core;
using NUnit.Framework;

namespace Asteroids.Session.Tests
{
    /// <summary>
    /// The session facade: pause and resume, restart order and failure handling, and how it fronts the
    /// <see cref="RunLifecycle"/> (pauses and reports once a run has ended, refuses to resume an ended run).
    /// </summary>
    [TestFixture]
    public sealed class GameSessionTests
    {
        private FakeSimulation _simulation;
        private FakeRunEndConfig _config;
        private FakeScore _score;
        private FakeWaveProgress _waves;
        private RecordingRunEvents _events;
        private RunLifecycle _lifecycle;
        private List<string> _log;

        [SetUp]
        public void SetUp()
        {
            _simulation = new FakeSimulation();
            _config = new FakeRunEndConfig { DefeatDelay = 1.5f, VictoryDelay = 1f };
            _score = new FakeScore();
            _waves = new FakeWaveProgress { Cleared = 3, Total = 5 };
            _events = new RecordingRunEvents();
            _lifecycle = new RunLifecycle(_config, _score, _waves);
            _log = new List<string>();
        }

        private FakeRestartable Restartable(RestartOrder order) => new(order, _log);

        /// <summary>A session whose restartables are <paramref name="others"/> plus the lifecycle, as the installer binds them.</summary>
        private GameSession Session(params IRestartable[] others)
        {
            var restartables = new List<IRestartable>(others) { _lifecycle };
            return new GameSession(_simulation, _lifecycle, _events, restartables);
        }

        [Test]
        public void PauseAndResume_SwitchTheSimulation()
        {
            var session = Session();

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
            var session = Session(
                Restartable(RestartOrder.Score),
                Restartable(RestartOrder.Enemies),
                Restartable(RestartOrder.Waves),
                Restartable(RestartOrder.Ship));

            session.Restart();

            Assert.That(_log, Is.EqualTo(new[] { "Enemies", "Ship", "Waves", "Score" }));
        }

        [Test]
        public void Restart_WhilePaused_Resumes()
        {
            var session = Session();
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
            var session = Session(enemies, ship, Restartable(RestartOrder.Score));
            session.Pause();

            var thrown = Assert.Throws<InvalidOperationException>(() => session.Restart());

            Assert.That(thrown, Is.SameAs(first));
            Assert.That(_log, Is.EqualTo(new[] { "Enemies", "Ship", "Score" }));
            Assert.That(_simulation.IsPaused, Is.False);
        }

        /// <summary>Even when something else fails to restart, the run itself must be playing again.</summary>
        [Test]
        public void Restart_OneFails_TheRunIsStillPlayingAgain()
        {
            var enemies = Restartable(RestartOrder.Enemies);
            enemies.Failure = new InvalidOperationException("boom");
            var session = Session(enemies);
            session.Finish(RunOutcome.Defeat);
            _lifecycle.Tick(2f);

            Assert.Throws<InvalidOperationException>(() => session.Restart());

            Assert.That(session.State.Value, Is.EqualTo(RunState.Playing));
            Assert.That(_simulation.IsPaused, Is.False);
        }

        [Test]
        public void State_IsTheLifecycleState()
        {
            var session = Session();

            Assert.That(session.State, Is.SameAs(_lifecycle.State));
        }

        [Test]
        public void Finish_DuringTheDelay_KeepsTheSimulationRunning()
        {
            var session = Session();

            session.Finish(RunOutcome.Defeat);
            _lifecycle.Tick(1f);

            Assert.That(session.State.Value.Phase, Is.EqualTo(RunPhase.Ending));
            Assert.That(_simulation.IsPaused, Is.False);
            Assert.That(_events.Ended, Is.Empty);
        }

        [Test]
        public void RunEnded_PausesAndReportsTheFrozenResultOnce()
        {
            var session = Session();
            _score.Points = 700;
            session.Finish(RunOutcome.Victory);
            _score.Points = 900;

            _lifecycle.Tick(1f);
            _lifecycle.Tick(1f);

            Assert.That(_simulation.IsPaused, Is.True);
            Assert.That(_events.Ended, Is.EqualTo(new[] { new RunResult(RunOutcome.Victory, 700, 3, 5) }));
        }

        /// <summary>A future pause menu calling Resume must not restart the world behind the end screen.</summary>
        [Test]
        public void Resume_AfterTheRunEnded_DoesNothing()
        {
            var session = Session();
            session.Finish(RunOutcome.Defeat);
            _lifecycle.Tick(2f);

            session.Resume();

            Assert.That(_simulation.IsPaused, Is.True);
        }

        /// <summary>Pausing during the end delay (a pause menu) and resuming must still be possible.</summary>
        [Test]
        public void Resume_DuringEnding_Works()
        {
            var session = Session();
            session.Finish(RunOutcome.Defeat);
            session.Pause();

            session.Resume();

            Assert.That(_simulation.IsPaused, Is.False);
        }

        [Test]
        public void Restart_AfterTheRunEnded_PlaysAndRunsAgain()
        {
            var session = Session();
            session.Finish(RunOutcome.Defeat);
            _lifecycle.Tick(2f);

            session.Restart();

            Assert.That(session.State.Value, Is.EqualTo(RunState.Playing));
            Assert.That(_simulation.IsPaused, Is.False);
        }

        [Test]
        public void Restart_DuringEnding_NeverPausesLater()
        {
            var session = Session();
            session.Finish(RunOutcome.Defeat);
            _lifecycle.Tick(1f);

            session.Restart();
            _lifecycle.Tick(10f);

            Assert.That(_simulation.IsPaused, Is.False);
            Assert.That(_events.Ended, Is.Empty);
        }

        [Test]
        public void Dispose_StopsReactingToTheLifecycle()
        {
            var session = Session();
            session.Dispose();

            session.Finish(RunOutcome.Defeat);
            _lifecycle.Tick(2f);

            Assert.That(_simulation.IsPaused, Is.False);
            Assert.That(_events.Ended, Is.Empty);
        }

        [Test]
        public void Constructor_TwoWithTheSameOrder_Throws()
        {
            var error = Assert.Throws<ArgumentException>(() =>
                Session(Restartable(RestartOrder.Ship), Restartable(RestartOrder.Ship)));

            StringAssert.Contains("Ship", error.Message);
        }

        [Test]
        public void Constructor_LifecycleNotAmongTheRestartables_Throws()
        {
            var error = Assert.Throws<ArgumentException>(() =>
                new GameSession(_simulation, _lifecycle, _events, Array.Empty<IRestartable>()));

            StringAssert.Contains(nameof(RunLifecycle), error.Message);
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            var restartables = new IRestartable[] { _lifecycle };

            Assert.Throws<ArgumentNullException>(() => new GameSession(null, _lifecycle, _events, restartables));
            Assert.Throws<ArgumentNullException>(() => new GameSession(_simulation, null, _events, restartables));
            Assert.Throws<ArgumentNullException>(() => new GameSession(_simulation, _lifecycle, null, restartables));
            Assert.Throws<ArgumentNullException>(() => new GameSession(_simulation, _lifecycle, _events, null));
            Assert.Throws<ArgumentException>(() =>
                new GameSession(_simulation, _lifecycle, _events, new IRestartable[] { _lifecycle, null }));
        }

        [Test]
        public void RestartOrder_PutsEnemiesBeforeTheShipAndTheShipBeforeTheWaves()
        {
            Assert.That(RestartOrder.Enemies, Is.LessThan(RestartOrder.Ship));
            Assert.That(RestartOrder.Ship, Is.LessThan(RestartOrder.Waves));
        }
    }
}

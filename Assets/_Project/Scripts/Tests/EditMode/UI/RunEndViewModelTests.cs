using System;
using System.Collections.Generic;
using Asteroids.Common;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using NUnit.Framework;

namespace Asteroids.UI.Tests
{
    /// <summary>
    /// The end screen is hidden until the run has Ended, then shows the outcome's title and the frozen result in the
    /// configured formats. Replay restarts the session once.
    /// </summary>
    [TestFixture]
    public sealed class RunEndViewModelTests
    {
        private sealed class FakeSession : IGameSession
        {
            public ObservableValue<RunState> StateSource { get; } = new(RunState.Playing);
            public int Restarts { get; private set; }

            public bool IsPaused => false;
            public IObservableValue<RunState> State => StateSource;

            public void Pause() { }
            public void Resume() { }
            public void Finish(RunOutcome outcome) { }

            public void Restart()
            {
                Restarts++;
                StateSource.Value = RunState.Playing;
            }
        }

        private sealed class TestScreenConfig : IRunEndScreenConfig
        {
            public string DefeatTitle { get; set; } = "LOST";
            public string VictoryTitle { get; set; } = "WON";
            public string ScoreFormat { get; set; } = "{0}";
            public string WavesFormat { get; set; } = "{0}/{1}";
        }

        private FakeSession _session;
        private TestScreenConfig _config;

        [SetUp]
        public void SetUp()
        {
            _session = new FakeSession();
            _config = new TestScreenConfig();
        }

        private RunEndViewModel Create() => new(_session, _config);

        private void End(RunOutcome outcome, int score = 0, int cleared = 0, int total = 5) =>
            _session.StateSource.Value = RunState.Ended(new RunResult(outcome, score, cleared, total));

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new RunEndViewModel(null, _config));
            Assert.Throws<ArgumentNullException>(() => _ = new RunEndViewModel(_session, null));
        }

        [Test]
        public void Constructor_InvalidConfig_Throws()
        {
            _config.WavesFormat = "{2}";

            Assert.Throws<InvalidOperationException>(() => Create());
        }

        [Test]
        public void WhilePlaying_IsHidden() => Assert.That(Create().IsVisible.Value, Is.False);

        /// <summary>The outcome is decided but the world still runs. The screen waits for the pause.</summary>
        [Test]
        public void WhileEnding_IsHidden()
        {
            var viewModel = Create();

            _session.StateSource.Value = RunState.Ending(new RunResult(RunOutcome.Defeat, 1, 1, 5));

            Assert.That(viewModel.IsVisible.Value, Is.False);
        }

        [Test]
        public void Defeat_ShowsTheDefeatTitleAndTheResult()
        {
            var viewModel = Create();

            End(RunOutcome.Defeat, score: 1200, cleared: 2, total: 5);

            Assert.That(viewModel.IsVisible.Value, Is.True);
            Assert.That(viewModel.Title.Value, Is.EqualTo("LOST"));
            Assert.That(viewModel.ScoreText.Value, Is.EqualTo("1200"));
            Assert.That(viewModel.WavesText.Value, Is.EqualTo("2/5"));
        }

        [Test]
        public void Victory_ShowsTheVictoryTitle()
        {
            var viewModel = Create();

            End(RunOutcome.Victory, cleared: 5, total: 5);

            Assert.That(viewModel.Title.Value, Is.EqualTo("WON"));
            Assert.That(viewModel.WavesText.Value, Is.EqualTo("5/5"));
        }

        /// <summary>A view model created after the run ended (a late view) still shows the result.</summary>
        [Test]
        public void CreatedAfterTheRunEnded_ShowsTheResult()
        {
            End(RunOutcome.Victory, score: 10);

            var viewModel = Create();

            Assert.That(viewModel.IsVisible.Value, Is.True);
            Assert.That(viewModel.ScoreText.Value, Is.EqualTo("10"));
        }

        /// <summary>A view that appears when it turns visible must never flash the previous run's texts.</summary>
        [Test]
        public void Texts_AreSetBeforeItBecomesVisible()
        {
            var viewModel = Create();
            End(RunOutcome.Defeat, score: 1);
            _session.Restart();
            var titleWhenShown = new List<string>();
            viewModel.IsVisible.Subscribe(visible =>
            {
                if (visible) titleWhenShown.Add(viewModel.Title.Value);
            });

            End(RunOutcome.Victory, score: 2);

            Assert.That(titleWhenShown, Is.EqualTo(new[] { "WON" }));
        }

        [Test]
        [SetCulture("de-DE")]
        public void Texts_UseTheInvariantCulture()
        {
            _config.ScoreFormat = "{0:N0}";
            var viewModel = Create();

            End(RunOutcome.Defeat, score: 1234567);

            Assert.That(viewModel.ScoreText.Value, Is.EqualTo("1,234,567"));
        }

        [Test]
        public void Replay_RestartsAndHides()
        {
            var viewModel = Create();
            End(RunOutcome.Defeat);

            viewModel.Replay();

            Assert.That(_session.Restarts, Is.EqualTo(1));
            Assert.That(viewModel.IsVisible.Value, Is.False);
        }

        /// <summary>A double click, or Submit and a click in the same frame, must restart once.</summary>
        [Test]
        public void Replay_Twice_RestartsOnce()
        {
            var viewModel = Create();
            End(RunOutcome.Defeat);

            viewModel.Replay();
            viewModel.Replay();

            Assert.That(_session.Restarts, Is.EqualTo(1));
        }

        [Test]
        public void Replay_WhileHidden_DoesNothing()
        {
            var viewModel = Create();

            viewModel.Replay();

            Assert.That(_session.Restarts, Is.Zero);
        }

        [Test]
        public void Dispose_StopsFollowingTheSession()
        {
            var viewModel = Create();
            viewModel.Dispose();

            End(RunOutcome.Defeat);

            Assert.That(viewModel.IsVisible.Value, Is.False);
        }
    }
}

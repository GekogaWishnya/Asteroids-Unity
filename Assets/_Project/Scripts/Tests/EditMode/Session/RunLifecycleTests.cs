using System;
using System.Collections.Generic;
using Asteroids.Core;
using NUnit.Framework;

namespace Asteroids.Session.Tests
{
    /// <summary>
    /// The run state machine on its own: Playing -> Ending (outcome decided, result frozen) -> Ended (delay passed),
    /// and back to Playing on restart. Pausing is not its job; <see cref="GameSessionTests"/> covers that.
    /// </summary>
    [TestFixture]
    public sealed class RunLifecycleTests
    {
        private FakeRunEndConfig _config;
        private FakeScore _score;
        private FakeWaveProgress _waves;

        [SetUp]
        public void SetUp()
        {
            _config = new FakeRunEndConfig { DefeatDelay = 1.5f, VictoryDelay = 1f };
            _score = new FakeScore();
            _waves = new FakeWaveProgress { Cleared = 2, Total = 5 };
        }

        private RunLifecycle Lifecycle() => new(_config, _score, _waves);

        /// <summary>Subscribes and records every state the lifecycle publishes, including the immediate one.</summary>
        private static List<RunState> Record(RunLifecycle lifecycle)
        {
            var states = new List<RunState>();
            lifecycle.State.Subscribe(states.Add);
            return states;
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new RunLifecycle(null, _score, _waves));
            Assert.Throws<ArgumentNullException>(() => new RunLifecycle(_config, null, _waves));
            Assert.Throws<ArgumentNullException>(() => new RunLifecycle(_config, _score, null));
        }

        [Test]
        public void Constructor_InvalidConfig_ThrowsListingTheProblem()
        {
            _config.DefeatDelay = -1f;

            var error = Assert.Throws<InvalidOperationException>(() => Lifecycle());

            StringAssert.Contains("Defeat delay", error.Message);
        }

        [Test]
        public void New_IsPlaying()
        {
            var lifecycle = Lifecycle();

            Assert.That(lifecycle.State.Value, Is.EqualTo(RunState.Playing));
            Assert.That(lifecycle.State.Value.HasResult, Is.False);
            Assert.That(lifecycle.Order, Is.EqualTo(RestartOrder.Run));
        }

        [Test]
        public void Tick_WhilePlaying_DoesNothing()
        {
            var lifecycle = Lifecycle();
            var states = Record(lifecycle);

            lifecycle.Tick(100f);

            Assert.That(states, Is.EqualTo(new[] { RunState.Playing }));
        }

        [Test]
        public void Finish_Defeat_EntersEndingWithTheCurrentResult()
        {
            var lifecycle = Lifecycle();
            _score.Points = 1200;

            lifecycle.Finish(RunOutcome.Defeat);

            var state = lifecycle.State.Value;
            Assert.That(state.Phase, Is.EqualTo(RunPhase.Ending));
            Assert.That(state.Result, Is.EqualTo(new RunResult(RunOutcome.Defeat, 1200, 2, 5)));
        }

        /// <summary>
        /// The ticks are binary-exact (1.25 + 0.25 = 1.5) on purpose. With 1.4 + 0.1 the float remainder is about
        /// 2e-8 above zero and the run ends one tick later, which is fine in play but not what this test pins down.
        /// </summary>
        [Test]
        public void Tick_EndsOnlyOnceTheDelayOfTheOutcomeHasPassed()
        {
            var lifecycle = Lifecycle();
            lifecycle.Finish(RunOutcome.Defeat);

            lifecycle.Tick(1.25f);
            Assert.That(lifecycle.State.Value.Phase, Is.EqualTo(RunPhase.Ending));

            lifecycle.Tick(0.25f);
            Assert.That(lifecycle.State.Value.Phase, Is.EqualTo(RunPhase.Ended));
        }

        [Test]
        public void Finish_Victory_UsesTheVictoryDelay()
        {
            var lifecycle = Lifecycle();
            lifecycle.Finish(RunOutcome.Victory);

            lifecycle.Tick(1f);

            Assert.That(lifecycle.State.Value.Phase, Is.EqualTo(RunPhase.Ended));
            Assert.That(lifecycle.State.Value.Result.Outcome, Is.EqualTo(RunOutcome.Victory));
        }

        /// <summary>
        /// The result shown at the end must be the one at the moment the outcome was decided. Projectiles in flight
        /// can still score and the waves still run during the delay.
        /// </summary>
        [Test]
        public void Result_IsFrozenWhenTheOutcomeIsDecided()
        {
            var lifecycle = Lifecycle();
            _score.Points = 100;
            lifecycle.Finish(RunOutcome.Defeat);

            _score.Points = 900;
            _waves.Cleared = 5;
            lifecycle.Tick(2f);

            Assert.That(lifecycle.State.Value.Result, Is.EqualTo(new RunResult(RunOutcome.Defeat, 100, 2, 5)));
        }

        /// <summary>
        /// "Let the system decide": the first outcome of a run wins. A projectile killing the last enemy during the
        /// defeat delay must not turn the run into a victory.
        /// </summary>
        [Test]
        public void Finish_Twice_FirstOutcomeWins()
        {
            var lifecycle = Lifecycle();
            lifecycle.Finish(RunOutcome.Defeat);

            lifecycle.Finish(RunOutcome.Victory);
            lifecycle.Tick(2f);
            lifecycle.Finish(RunOutcome.Victory);

            Assert.That(lifecycle.State.Value.Result.Outcome, Is.EqualTo(RunOutcome.Defeat));
        }

        /// <summary>A second Finish must not restart the delay either.</summary>
        [Test]
        public void Finish_DuringEnding_DoesNotExtendTheDelay()
        {
            var lifecycle = Lifecycle();
            lifecycle.Finish(RunOutcome.Defeat);
            lifecycle.Tick(1f);

            lifecycle.Finish(RunOutcome.Defeat);
            lifecycle.Tick(0.5f);

            Assert.That(lifecycle.State.Value.Phase, Is.EqualTo(RunPhase.Ended));
        }

        [Test]
        public void Finish_WithNoDelay_PublishesEndingThenEndedAtOnce()
        {
            _config.DefeatDelay = 0f;
            var lifecycle = Lifecycle();
            var states = Record(lifecycle);

            lifecycle.Finish(RunOutcome.Defeat);

            var result = new RunResult(RunOutcome.Defeat, 0, 2, 5);
            Assert.That(states,
                Is.EqualTo(new[] { RunState.Playing, RunState.Ending(result), RunState.Ended(result) }));
        }

        [Test]
        public void Ended_IsPublishedOnce()
        {
            var lifecycle = Lifecycle();
            var states = Record(lifecycle);
            lifecycle.Finish(RunOutcome.Victory);

            lifecycle.Tick(1f);
            lifecycle.Tick(1f);

            Assert.That(states.FindAll(state => state.Phase == RunPhase.Ended), Has.Count.EqualTo(1));
        }

        [Test]
        public void Restart_DuringEnding_CancelsTheEnd()
        {
            var lifecycle = Lifecycle();
            lifecycle.Finish(RunOutcome.Defeat);
            lifecycle.Tick(1f);

            lifecycle.Restart();
            lifecycle.Tick(10f);

            Assert.That(lifecycle.State.Value, Is.EqualTo(RunState.Playing));
        }

        [Test]
        public void Restart_AfterEnded_AcceptsANewOutcomeWithAFullDelay()
        {
            var lifecycle = Lifecycle();
            lifecycle.Finish(RunOutcome.Defeat);
            lifecycle.Tick(2f);

            lifecycle.Restart();
            _score.Points = 50;
            lifecycle.Finish(RunOutcome.Victory);
            lifecycle.Tick(0.9f);

            Assert.That(lifecycle.State.Value.Phase, Is.EqualTo(RunPhase.Ending));
            Assert.That(lifecycle.State.Value.Result, Is.EqualTo(new RunResult(RunOutcome.Victory, 50, 2, 5)));
        }

        /// <summary>A subscriber that restarts the run as soon as it is Ending must not be overruled by End.</summary>
        [Test]
        public void Finish_SubscriberRestartsDuringEnding_StaysPlaying()
        {
            _config.DefeatDelay = 0f;
            var lifecycle = Lifecycle();
            lifecycle.State.Subscribe(state =>
            {
                if (state.Phase == RunPhase.Ending) lifecycle.Restart();
            });

            lifecycle.Finish(RunOutcome.Defeat);

            Assert.That(lifecycle.State.Value, Is.EqualTo(RunState.Playing));
        }

        [Test]
        public void Finish_UndefinedOutcome_Throws()
        {
            var lifecycle = Lifecycle();

            Assert.Throws<ArgumentOutOfRangeException>(() => lifecycle.Finish((RunOutcome)42));
            Assert.That(lifecycle.State.Value, Is.EqualTo(RunState.Playing));
        }

        [Test]
        public void RunState_Playing_HasNoResult() =>
            Assert.Throws<InvalidOperationException>(() => _ = RunState.Playing.Result);

        [Test]
        public void RestartOrder_PutsTheRunLast()
        {
            foreach (RestartOrder order in Enum.GetValues(typeof(RestartOrder)))
            {
                if (order != RestartOrder.Run) Assert.That(order, Is.LessThan(RestartOrder.Run));
            }
        }
    }
}
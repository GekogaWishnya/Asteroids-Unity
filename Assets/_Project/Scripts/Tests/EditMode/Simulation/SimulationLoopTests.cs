using System;
using System.Collections.Generic;
using Asteroids.Common;
using Asteroids.Core;
using NUnit.Framework;

namespace Asteroids.Simulation.Tests
{
    [TestFixture]
    public sealed class SimulationLoopTests
    {
        private sealed class FakeTime : ISimulationTime
        {
            public float DeltaTime { get; set; } = 0.01f;
        }

        private sealed class RecordingLog : IErrorLog
        {
            public List<(Exception Failure, string Context)> Reports { get; } = new();

            public void Report(Exception failure, string context) => Reports.Add((failure, context));
        }

        private sealed class Step : ISimulationStep
        {
            private readonly string _name;
            private readonly List<string> _log;

            public Step(string name, List<string> log)
            {
                _name = name;
                _log = log;
            }

            public Action OnTick { get; set; }
            public float LastDelta { get; private set; }

            public void Tick(float deltaTime)
            {
                LastDelta = deltaTime;
                _log.Add(_name);
                OnTick?.Invoke();
            }
        }

        private FakeTime _time;
        private RecordingLog _errors;
        private SimulationLoop _loop;
        private List<string> _log;

        [SetUp]
        public void SetUp()
        {
            _time = new FakeTime();
            _errors = new RecordingLog();
            _loop = new SimulationLoop(_time, _errors);
            _log = new List<string>();
        }

        [Test]
        public void Constructor_NullArgument_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new SimulationLoop(null, _errors));
            Assert.Throws<ArgumentNullException>(() => new SimulationLoop(_time, null));
        }

        [Test]
        public void Tick_RunsPhasesInOrderAndStepsInRegistrationOrder()
        {
            _loop.Register(SimulationPhase.Logic, new Step("logic", _log));
            _loop.Register(SimulationPhase.Input, new Step("input 1", _log));
            _loop.Register(SimulationPhase.Collision, new Step("collision", _log));
            _loop.Register(SimulationPhase.Input, new Step("input 2", _log));
            _loop.Register(SimulationPhase.Motion, new Step("motion", _log));
            _loop.Register(SimulationPhase.Constraint, new Step("constraint", _log));

            _loop.Tick();

            Assert.That(_log, Is.EqualTo(new[] { "input 1", "input 2", "motion", "constraint", "collision", "logic" }));
        }

        [Test]
        public void Tick_LongFrame_IsClampedToTheMaximumDelta()
        {
            var step = new Step("a", _log);
            _loop.Register(SimulationPhase.Logic, step);
            _time.DeltaTime = 1f;

            _loop.Tick();

            Assert.That(step.LastDelta, Is.EqualTo(1f / 20f).Within(1e-6f));
        }

        [Test]
        public void Tick_NoTimePassed_RunsNothing()
        {
            _loop.Register(SimulationPhase.Logic, new Step("a", _log));
            _time.DeltaTime = 0f;

            _loop.Tick();

            Assert.That(_log, Is.Empty);
        }

        [Test]
        public void Tick_Paused_RunsNothing()
        {
            _loop.Register(SimulationPhase.Logic, new Step("a", _log));
            _loop.IsPaused = true;

            _loop.Tick();

            Assert.That(_log, Is.Empty);
        }

        [Test]
        public void Tick_PausedThenResumed_RunsAgain()
        {
            _loop.Register(SimulationPhase.Logic, new Step("a", _log));

            _loop.IsPaused = true;
            _loop.Tick();
            _loop.IsPaused = false;
            _loop.Tick();

            Assert.That(_log, Has.Count.EqualTo(1));
        }

        [Test]
        public void Tick_AStepThrows_LaterStepsAndPhasesStillRunAndTheFailureIsReported()
        {
            var failure = new InvalidOperationException("boom");
            _loop.Register(SimulationPhase.Motion, new Step("broken", _log) { OnTick = () => throw failure });
            _loop.Register(SimulationPhase.Motion, new Step("motion", _log));
            _loop.Register(SimulationPhase.Logic, new Step("logic", _log));

            Assert.DoesNotThrow(() => _loop.Tick());

            Assert.That(_log, Is.EqualTo(new[] { "broken", "motion", "logic" }));
            Assert.That(_errors.Reports.Count, Is.EqualTo(1));
            Assert.That(_errors.Reports[0].Failure, Is.SameAs(failure));
            Assert.That(_errors.Reports[0].Context,
                Does.Contain(nameof(Step)).And.Contain(nameof(SimulationPhase.Motion)));
        }

        [Test]
        public void Tick_StepFailsEveryTick_IsReportedAtMostOncePerIntervalWithTheSuppressedCount()
        {
            _loop.Register(SimulationPhase.Logic, new Step("broken", _log) { OnTick = () => throw new Exception() });
            // A power of two below the clamp, so the simulated time adds up exactly.
            _time.DeltaTime = 1f / 32f;
            var ticksPerInterval = (int)Math.Round(SimulationLoop.FailureReportInterval * 32f);

            for (var i = 0; i < ticksPerInterval; i++) _loop.Tick();
            Assert.That(_errors.Reports.Count, Is.EqualTo(1));

            _loop.Tick();
            Assert.That(_errors.Reports.Count, Is.EqualTo(2));
            Assert.That(_errors.Reports[1].Context, Does.Contain((ticksPerInterval - 1).ToString()));
        }

        [Test]
        public void Tick_AStepThrows_TheLoopKeepsTickingOnLaterFrames()
        {
            var calls = 0;
            _loop.Register(SimulationPhase.Logic, new Step("broken", _log) { OnTick = () => throw new Exception() });
            _loop.Register(SimulationPhase.Logic, new Step("counter", _log) { OnTick = () => calls++ });

            _loop.Tick();
            _loop.Tick();

            Assert.That(calls, Is.EqualTo(2));
        }

        [Test]
        public void Register_DuringTick_RunsFromTheNextTick()
        {
            var late = new Step("late", _log);
            _loop.Register(SimulationPhase.Input, new Step("first", _log)
            {
                OnTick = () => _loop.Register(SimulationPhase.Logic, late)
            });

            _loop.Tick();
            Assert.That(_log, Is.EqualTo(new[] { "first" }));

            _log.Clear();
            _loop.Tick();
            Assert.That(_log, Does.Contain("late"));
        }

        [Test]
        public void Dispose_DuringTick_StepStillRunsThisTickAndStopsAfter()
        {
            IDisposable handle = null;
            _loop.Register(SimulationPhase.Input, new Step("remover", _log) { OnTick = () => handle.Dispose() });
            handle = _loop.Register(SimulationPhase.Logic, new Step("removed", _log));

            _loop.Tick();
            Assert.That(_log, Does.Contain("removed"));

            _log.Clear();
            _loop.Tick();
            Assert.That(_log, Is.EqualTo(new[] { "remover" }));
        }

        [Test]
        public void Dispose_Twice_DoesNothing()
        {
            var handle = _loop.Register(SimulationPhase.Logic, new Step("a", _log));
            handle.Dispose();

            Assert.DoesNotThrow(() => handle.Dispose());
            _loop.Tick();
            Assert.That(_log, Is.Empty);
        }

        [Test]
        public void Tick_CalledFromAStep_IsReportedAndDoesNotNest()
        {
            var nested = 0;
            _loop.Register(SimulationPhase.Logic, new Step("a", _log)
            {
                OnTick = () =>
                {
                    nested++;
                    _loop.Tick();
                }
            });

            _loop.Tick();

            Assert.That(nested, Is.EqualTo(1));
            Assert.That(_errors.Reports.Count, Is.EqualTo(1));
            Assert.That(_errors.Reports[0].Failure, Is.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Register_NullStepOrUnknownPhase_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _loop.Register(SimulationPhase.Logic, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => _loop.Register((SimulationPhase)99, new Step("a", _log)));
        }
    }
}
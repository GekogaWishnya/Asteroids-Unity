using System;
using System.Collections.Generic;
using Asteroids.Common;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Zenject;

namespace Asteroids.Ticking.Tests
{
    /// <summary>
    /// <see cref="FrameTicker{TTick}"/> replaces Zenject's <c>ITickable</c>. What it must keep from it: one tick
    /// per frame, nothing before the scene starts or after it ends. What it must add: a throwing tick must not end
    /// the async loop for good, which is what would happen to an unguarded <c>await</c> loop.
    /// </summary>
    [TestFixture]
    public sealed class FrameTickerTests
    {
        private sealed class CountingTick : IFrameTick
        {
            public int Ticks { get; private set; }
            public Action OnTick { get; set; }

            public void Tick()
            {
                Ticks++;
                OnTick?.Invoke();
            }
        }

        private sealed class RecordingLog : IErrorLog
        {
            public List<(Exception Failure, string Context)> Reports { get; } = new();

            public void Report(Exception failure, string context) => Reports.Add((failure, context));
        }

        private ManualFrames _frames;
        private RecordingLog _errors;
        private CountingTick _tick;
        private FrameTicker<CountingTick> _ticker;

        [SetUp]
        public void SetUp()
        {
            _frames = new ManualFrames();
            _errors = new RecordingLog();
            _tick = new CountingTick();
            _ticker = new FrameTicker<CountingTick>(_tick, PlayerLoopTiming.Update, _frames, _errors);
        }

        [TearDown]
        public void TearDown() => _ticker.Dispose();

        [Test]
        public void Constructor_NullArgument_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new FrameTicker<CountingTick>(null, PlayerLoopTiming.Update, _frames, _errors));
            Assert.Throws<ArgumentNullException>(() =>
                new FrameTicker<CountingTick>(_tick, PlayerLoopTiming.Update, null, _errors));
            Assert.Throws<ArgumentNullException>(() =>
                new FrameTicker<CountingTick>(_tick, PlayerLoopTiming.Update, _frames, null));
        }

        [Test]
        public void BeforeInitialize_NothingWaitsAndNothingTicks()
        {
            Assert.That(_frames.IsWaiting, Is.False);
            Assert.That(_ticker.IsRunning, Is.False);
            Assert.That(_tick.Ticks, Is.Zero);
        }

        /// <summary>Initialize only starts waiting; the first tick comes with the first frame, like ITickable.</summary>
        [Test]
        public void Initialize_WaitsForTheFirstFrameWithoutTicking()
        {
            _ticker.Initialize();

            Assert.That(_frames.IsWaiting, Is.True);
            Assert.That(_tick.Ticks, Is.Zero);
        }

        [Test]
        public void EachFrame_TicksExactlyOnce()
        {
            _ticker.Initialize();

            _frames.Frame();
            Assert.That(_tick.Ticks, Is.EqualTo(1));

            _frames.Frame();
            _frames.Frame();
            Assert.That(_tick.Ticks, Is.EqualTo(3));
        }

        [Test]
        public void EveryWait_IsOnTheScheduledTiming()
        {
            var ticker = new FrameTicker<CountingTick>(_tick, PlayerLoopTiming.PreLateUpdate, _frames, _errors);
            ticker.Initialize();
            _frames.Frame();
            _frames.Frame();
            ticker.Dispose();

            Assert.That(_frames.Requested, Is.All.EqualTo(PlayerLoopTiming.PreLateUpdate));
        }

        /// <summary>
        /// The important one. Without the guard the exception would escape the async method, the UniTaskVoid would
        /// end, and the system would silently stop for the rest of the scene.
        /// </summary>
        [Test]
        public void ThrowingTick_IsReportedAndTicksAgainOnTheNextFrame()
        {
            var failure = new InvalidOperationException("boom");
            _tick.OnTick = () => throw failure;
            _ticker.Initialize();

            _frames.Frame();
            _frames.Frame();

            Assert.That(_tick.Ticks, Is.EqualTo(2));
            Assert.That(_errors.Reports, Has.Count.EqualTo(2));
            Assert.That(_errors.Reports[0].Failure, Is.SameAs(failure));
            Assert.That(_errors.Reports[0].Context, Does.Contain(nameof(CountingTick)));
            Assert.That(_frames.IsWaiting, Is.True);
        }

        [Test]
        public void Dispose_StopsTheLoop()
        {
            _ticker.Initialize();
            _frames.Frame();

            _ticker.Dispose();

            Assert.That(_frames.IsWaiting, Is.False);
            Assert.That(_ticker.IsRunning, Is.False);
            Assert.That(_tick.Ticks, Is.EqualTo(1));
        }

        /// <summary>A tick may end its own scene, for example a restart that disposes the container.</summary>
        [Test]
        public void DisposeFromInsideTheTick_NoFurtherTicks()
        {
            _tick.OnTick = () => _ticker.Dispose();
            _ticker.Initialize();

            _frames.Frame();

            Assert.That(_tick.Ticks, Is.EqualTo(1));
            Assert.That(_frames.IsWaiting, Is.False);
            Assert.That(_errors.Reports, Is.Empty);
        }

        [Test]
        public void Dispose_Twice_DoesNothing()
        {
            _ticker.Initialize();
            _ticker.Dispose();

            Assert.DoesNotThrow(() => _ticker.Dispose());
        }

        [Test]
        public void Dispose_BeforeInitialize_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _ticker.Dispose());
        }

        [Test]
        public void Initialize_AfterDispose_Throws()
        {
            _ticker.Dispose();

            Assert.Throws<ObjectDisposedException>(() => _ticker.Initialize());
        }

        [Test]
        public void Initialize_Twice_Throws()
        {
            _ticker.Initialize();

            Assert.Throws<InvalidOperationException>(() => _ticker.Initialize());
        }

        /// <summary>The Zenject path: the binding ticks the instance bound in the container, at the given timing.</summary>
        [Test]
        public void BindFrameTick_TicksTheBoundInstanceAfterInitialize()
        {
            var container = new DiContainer();
            container.Bind<IFrameSource>().FromInstance(_frames);
            container.Bind<IErrorLog>().FromInstance(_errors);
            container.Bind<CountingTick>().FromInstance(_tick);

            container.BindFrameTick<CountingTick>(PlayerLoopTiming.PreUpdate);

            var initializables = container.ResolveAll<IInitializable>();
            Assert.That(initializables, Has.Count.EqualTo(1));
            initializables[0].Initialize();
            _frames.Frame();

            Assert.That(_tick.Ticks, Is.EqualTo(1));
            Assert.That(_frames.Requested, Is.All.EqualTo(PlayerLoopTiming.PreUpdate));

            foreach (var disposable in container.ResolveAll<IDisposable>()) disposable.Dispose();
            Assert.That(_frames.IsWaiting, Is.False);
        }

        [Test]
        public void BindFrameTick_NullContainer_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                FrameTickBindings.BindFrameTick<CountingTick>(null, PlayerLoopTiming.Update));
        }
    }
}

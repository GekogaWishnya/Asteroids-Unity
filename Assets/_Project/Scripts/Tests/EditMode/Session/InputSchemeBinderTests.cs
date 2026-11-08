using System;
using Asteroids.Core;
using NUnit.Framework;

namespace Asteroids.Session.Tests
{
    /// <summary>
    /// The active input scheme follows the run: Playing -> Gameplay, Ending -> None (the ship is locked once the
    /// outcome is decided), Ended -> UI. Uses a real <see cref="GameSession"/> over a real <see cref="RunLifecycle"/>.
    /// </summary>
    [TestFixture]
    public sealed class InputSchemeBinderTests
    {
        private RunLifecycle _lifecycle;
        private GameSession _session;
        private FakeInputSchemes _schemes;
        private InputSchemeBinder _binder;

        [SetUp]
        public void SetUp()
        {
            _lifecycle = new RunLifecycle(new FakeRunEndConfig { DefeatDelay = 1f, VictoryDelay = 1f },
                new FakeScore(), new FakeWaveProgress());
            _session = new GameSession(new FakeSimulation(), _lifecycle, new RecordingRunEvents(),
                new IRestartable[] { _lifecycle });
            _schemes = new FakeInputSchemes();
            _binder = new InputSchemeBinder(_session, _schemes);
        }

        [TearDown]
        public void TearDown()
        {
            _binder.Dispose();
            _session.Dispose();
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new InputSchemeBinder(null, _schemes));
            Assert.Throws<ArgumentNullException>(() => _ = new InputSchemeBinder(_session, null));
        }

        [Test]
        public void Construct_DoesNotTouchTheSchemeBeforeInitialize() =>
            Assert.That(_schemes.Activated, Is.Empty);

        /// <summary>Without this the ship would be deaf until the first state change.</summary>
        [Test]
        public void Initialize_AppliesTheCurrentState()
        {
            _binder.Initialize();

            Assert.That(_schemes.Active.Value, Is.EqualTo(InputScheme.Gameplay));
        }

        [Test]
        public void TheSchemeFollowsAWholeRun()
        {
            _binder.Initialize();

            _session.Finish(RunOutcome.Victory);
            Assert.That(_schemes.Active.Value, Is.EqualTo(InputScheme.None));

            _lifecycle.Tick(1f);
            Assert.That(_schemes.Active.Value, Is.EqualTo(InputScheme.UI));

            _session.Restart();
            Assert.That(_schemes.Active.Value, Is.EqualTo(InputScheme.Gameplay));
        }

        [Test]
        public void Initialize_Twice_DoesNotSubscribeTwice()
        {
            _binder.Initialize();
            _binder.Initialize();
            _schemes.Activated.Clear();

            _session.Finish(RunOutcome.Defeat);

            Assert.That(_schemes.Activated, Is.EqualTo(new[] { InputScheme.None }));
        }

        [Test]
        public void Dispose_StopsFollowing()
        {
            _binder.Initialize();
            _binder.Dispose();

            _session.Finish(RunOutcome.Defeat);

            Assert.That(_schemes.Active.Value, Is.EqualTo(InputScheme.Gameplay));
        }

        [TestCase(RunPhase.Playing, InputScheme.Gameplay)]
        [TestCase(RunPhase.Ending, InputScheme.None)]
        [TestCase(RunPhase.Ended, InputScheme.UI)]
        public void SchemeFor_MapsEveryPhase(RunPhase phase, InputScheme expected) =>
            Assert.That(InputSchemeBinder.SchemeFor(phase), Is.EqualTo(expected));

        [Test]
        public void SchemeFor_UndefinedPhase_Throws() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => InputSchemeBinder.SchemeFor((RunPhase)42));
    }
}

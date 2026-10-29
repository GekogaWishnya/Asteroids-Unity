using System;
using Asteroids.Core;
using NUnit.Framework;
using static Asteroids.Waves.Tests.Build;

namespace Asteroids.Waves.Tests
{
    [TestFixture]
    public sealed class WaveDirectorRegistrationTests
    {
        private FakeSimulationLoop _loop;
        private FakeEnemyData _data;
        private WaveDirector _director;

        [SetUp]
        public void SetUp()
        {
            _loop = new FakeSimulationLoop();
            _data = new FakeEnemyData("A");
            _director = new WaveDirector(Level(Wave(0f, Entry("A"))), _data, new RecordingWaveEvents());
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new WaveDirectorRegistration(null, _director));
            Assert.Throws<ArgumentNullException>(() => new WaveDirectorRegistration(_loop, null));
        }

        [Test]
        public void Initialize_RegistersTheDirectorInTheLogicPhase()
        {
            new WaveDirectorRegistration(_loop, _director).Initialize();

            Assert.That(_loop.LiveCount(SimulationPhase.Logic), Is.EqualTo(1));

            _loop.Tick(0.1f);

            Assert.That(_data.Spawned, Has.Count.EqualTo(1));
        }

        [Test]
        public void Construct_DoesNotRegisterBeforeInitialize()
        {
            new WaveDirectorRegistration(_loop, _director);

            Assert.That(_loop.LiveCount(SimulationPhase.Logic), Is.EqualTo(0));
        }

        [Test]
        public void Dispose_RemovesTheRegistration()
        {
            var registration = new WaveDirectorRegistration(_loop, _director);
            registration.Initialize();

            registration.Dispose();
            _loop.Tick(0.1f);

            Assert.That(_loop.LiveCount(SimulationPhase.Logic), Is.EqualTo(0));
            Assert.That(_data.Spawned, Is.Empty);
        }

        [Test]
        public void Dispose_WithoutInitialize_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new WaveDirectorRegistration(_loop, _director).Dispose());
        }
    }
}

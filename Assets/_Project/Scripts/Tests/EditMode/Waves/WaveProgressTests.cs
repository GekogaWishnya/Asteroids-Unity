using Asteroids.Core;
using NUnit.Framework;
using static Asteroids.Waves.Tests.Build;

namespace Asteroids.Waves.Tests
{
    /// <summary>
    /// <see cref="WaveDirector"/> as <see cref="IWaveProgress"/>. The end screen shows waves cleared, not waves
    /// reached, so a run lost during wave 3 of 5 must read 2/5 and a completed level 5/5.
    /// </summary>
    [TestFixture]
    public sealed class WaveProgressTests
    {
        private FakeEnemyData _data;

        [SetUp]
        public void SetUp() => _data = new FakeEnemyData("A");

        private IWaveProgress Progress(out WaveDirector director, int waves)
        {
            var list = new TestWave[waves];
            for (var i = 0; i < waves; i++) list[i] = Wave(0f, Entry("A"));
            director = new WaveDirector(Level(list), _data, new RecordingWaveEvents());
            return director;
        }

        /// <summary>One tick with no delay starts the wave and spawns its enemy; killing it and ticking clears it.</summary>
        private void ClearOneWave(WaveDirector director)
        {
            director.Tick(0.1f);
            _data.KillAll();
            director.Tick(0.1f);
        }

        [Test]
        public void New_HasClearedNothing()
        {
            var progress = Progress(out _, 3);

            Assert.That(progress.Cleared, Is.EqualTo(0));
            Assert.That(progress.Total, Is.EqualTo(3));
        }

        [Test]
        public void Cleared_DoesNotCountTheWaveInProgress()
        {
            var progress = Progress(out var director, 3);

            ClearOneWave(director);
            director.Tick(0.1f);

            Assert.That(director.WaveNumber, Is.EqualTo(2));
            Assert.That(progress.Cleared, Is.EqualTo(1));
        }

        [Test]
        public void Cleared_EqualsTheTotalOnceTheLevelIsCompleted()
        {
            var progress = Progress(out var director, 2);

            ClearOneWave(director);
            ClearOneWave(director);

            Assert.That(director.Stage, Is.EqualTo(WaveStage.Completed));
            Assert.That(progress.Cleared, Is.EqualTo(2));
        }

        [Test]
        public void Restart_ResetsCleared()
        {
            var progress = Progress(out var director, 2);
            ClearOneWave(director);

            director.Restart();

            Assert.That(progress.Cleared, Is.EqualTo(0));
        }
    }
}

using System;
using Asteroids.Core.IConfigs;
using NUnit.Framework;
using static Asteroids.Waves.Tests.Build;

namespace Asteroids.Waves.Tests
{
    [TestFixture]
    public sealed class WaveDirectorTests
    {
        private FakeEnemyData _data;
        private RecordingWaveEvents _events;

        [SetUp]
        public void SetUp()
        {
            _data = new FakeEnemyData("A", "B");
            _events = new RecordingWaveEvents();
        }

        private WaveDirector Director(TestLevel level) => new(level, _data, _events);

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            var level = Level(Wave(0f, Entry()));

            Assert.Throws<ArgumentNullException>(() => new WaveDirector(null, _data, _events));
            Assert.Throws<ArgumentNullException>(() => new WaveDirector(level, null, _events));
            Assert.Throws<ArgumentNullException>(() => new WaveDirector(level, _data, null));
        }

        [Test]
        public void Constructor_LevelWithoutWaves_Throws()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => Director(Level()));

            StringAssert.Contains("no waves", ex.Message);
        }

        [Test]
        public void Constructor_UnknownEnemyType_ThrowsNamingTheTypeAndThePlace()
        {
            var level = Level(Wave(0f, Entry("A")), Wave(0f, Entry("A"), Entry("Ghost")));

            var ex = Assert.Throws<InvalidOperationException>(() => Director(level));

            StringAssert.Contains("Ghost", ex.Message);
            StringAssert.Contains("Wave 2, entry 2", ex.Message);
        }

        [Test]
        public void New_WaitsForTheFirstWave()
        {
            var director = Director(Level(Wave(1f, Entry()), Wave(1f, Entry())));

            Assert.That(director.Stage, Is.EqualTo(WaveStage.Waiting));
            Assert.That(director.WaveNumber, Is.EqualTo(1));
            Assert.That(director.WaveCount, Is.EqualTo(2));
            Assert.That(_events.Log, Is.Empty);
        }

        [Test]
        public void Tick_BeforeTheDelayElapsed_DoesNothing()
        {
            var director = Director(Level(Wave(2f, Entry())));

            director.Tick(1.9f);

            Assert.That(director.Stage, Is.EqualTo(WaveStage.Waiting));
            Assert.That(_data.Spawned, Is.Empty);
            Assert.That(_events.Log, Is.Empty);
        }

        [Test]
        public void Tick_WhenTheDelayElapses_StartsTheWaveAndSpawnsWhatIsDue()
        {
            var director = Director(Level(Wave(1f, Entry("A", count: 2))));

            director.Tick(1f);

            Assert.That(_events.Log, Is.EqualTo(new[] { "started 1/1" }));
            Assert.That(_data.Spawned, Has.Count.EqualTo(2));
            Assert.That(director.Stage, Is.EqualTo(WaveStage.Clearing));
        }

        [Test]
        public void Tick_SpawnsAnEntryOnItsSchedule()
        {
            var director = Director(Level(Wave(0f, Entry("A", count: 3, start: 1f, interval: 2f))));

            Tick(director, 0.5f, 2);
            Assert.That(_data.Spawned, Has.Count.EqualTo(1));

            Tick(director, 0.5f, 3);
            Assert.That(_data.Spawned, Has.Count.EqualTo(1));

            Tick(director, 0.5f, 1);
            Assert.That(_data.Spawned, Has.Count.EqualTo(2));

            Tick(director, 0.5f, 4);
            Assert.That(_data.Spawned, Has.Count.EqualTo(3));
            Assert.That(director.Stage, Is.EqualTo(WaveStage.Clearing));
        }

        [Test]
        public void Tick_TwoEntries_SpawnEachOfItsOwnType()
        {
            var director = Director(Level(Wave(
                0f,
                Entry("A", count: 2),
                Entry("B", count: 1, start: 1f))));

            director.Tick(0.5f);
            Assert.That(_data.Spawned, Is.EqualTo(new[] { new EnemyTypeId("A"), new EnemyTypeId("A") }));

            director.Tick(0.5f);
            Assert.That(_data.Spawned, Has.Count.EqualTo(3));
            Assert.That(_data.Spawned[2], Is.EqualTo(new EnemyTypeId("B")));
        }

        [Test]
        public void Tick_HugeDelta_SpawnsEverythingDueAtOnceAndKeepsTheLeftover()
        {
            var director = Director(Level(Wave(1f, Entry("A", count: 3, start: 2f, interval: 3f))));

            director.Tick(10f);

            // 10 - 1 = 9 seconds into the wave: spawns at 2, 5 and 8 are all due.
            Assert.That(_data.Spawned, Has.Count.EqualTo(3));
        }

        [Test]
        public void Tick_NothingAliveButSpawnsPending_DoesNotClearTheWave()
        {
            _data.SpawnAddsToAlive = false;
            var director = Director(Level(Wave(0f, Entry("A", count: 2, interval: 5f))));

            director.Tick(0.5f);

            Assert.That(_data.Spawned, Has.Count.EqualTo(1));
            Assert.That(director.Stage, Is.EqualTo(WaveStage.Spawning));
            Assert.That(_events.Log, Is.EqualTo(new[] { "started 1/1" }));

            director.Tick(4.5f);

            Assert.That(_data.Spawned, Has.Count.EqualTo(2));
            Assert.That(_events.Log, Is.EqualTo(new[] { "started 1/1", "cleared 1", "completed 1" }));
        }

        [Test]
        public void Tick_EnemiesStillAlive_KeepsTheWaveOpenUntilTheyAreGone()
        {
            var director = Director(Level(Wave(0f, Entry("A", count: 2))));

            director.Tick(0.1f);
            director.Tick(0.1f);

            Assert.That(director.Stage, Is.EqualTo(WaveStage.Clearing));
            Assert.That(_events.Log, Is.EqualTo(new[] { "started 1/1" }));

            _data.KillAll();
            director.Tick(0.1f);

            Assert.That(_events.Log, Is.EqualTo(new[] { "started 1/1", "cleared 1", "completed 1" }));
        }

        [Test]
        public void Tick_NextWave_CountsItsDelayFromTheMomentTheWaveWasCleared()
        {
            var director = Director(Level(Wave(0f, Entry("A")), Wave(2f, Entry("B"))));
            director.Tick(0.1f);
            _data.KillAll();
            director.Tick(0.1f);
            Assert.That(director.Stage, Is.EqualTo(WaveStage.Waiting));
            Assert.That(director.WaveNumber, Is.EqualTo(2));

            director.Tick(1.5f);
            Assert.That(_events.Log, Is.EqualTo(new[] { "started 1/2", "cleared 1" }));

            director.Tick(0.5f);

            Assert.That(_events.Log, Is.EqualTo(new[] { "started 1/2", "cleared 1", "started 2/2" }));
            Assert.That(_data.Spawned[1], Is.EqualTo(new EnemyTypeId("B")));
        }

        [Test]
        public void Tick_LastWaveCleared_CompletesTheLevelOnce()
        {
            var director = Director(Level(Wave(0f, Entry("A")), Wave(0f, Entry("B"))));
            director.Tick(0.1f);
            _data.KillAll();
            director.Tick(0.1f);
            director.Tick(0.1f);
            _data.KillAll();
            director.Tick(0.1f);

            Assert.That(director.Stage, Is.EqualTo(WaveStage.Completed));
            Assert.That(director.WaveNumber, Is.EqualTo(2));
            Assert.That(_events.Log, Is.EqualTo(new[]
            {
                "started 1/2", "cleared 1", "started 2/2", "cleared 2", "completed 2"
            }));

            director.Tick(10f);
            director.Tick(10f);

            Assert.That(_events.Log, Has.Count.EqualTo(5));
            Assert.That(_data.Spawned, Has.Count.EqualTo(2));
        }

        [Test]
        public void Tick_EverythingDeadTheMomentTheLastOneSpawns_ClearsInTheSameTick()
        {
            _data.SpawnAddsToAlive = false;
            var director = Director(Level(Wave(0f, Entry("A", count: 2))));

            director.Tick(0.1f);

            Assert.That(_events.Log, Is.EqualTo(new[] { "started 1/1", "cleared 1", "completed 1" }));
        }

        [Test]
        public void Tick_SpawnFails_PropagatesAndRetriesNextTickWithoutCountingTheFailedSpawn()
        {
            var director = Director(Level(Wave(0f, Entry("A", count: 1))));
            _data.FailOnSpawn = true;

            Assert.Throws<InvalidOperationException>(() => director.Tick(0.1f));
            Assert.That(_data.Spawned, Is.Empty);

            _data.FailOnSpawn = false;
            director.Tick(0.1f);

            Assert.That(_data.Spawned, Has.Count.EqualTo(1));
            Assert.That(_events.Log, Is.EqualTo(new[] { "started 1/1" }));
        }

        [Test]
        public void Level_ChangedAfterConstruction_DoesNotDisturbARunningLevel()
        {
            var entry = Entry("A", count: 1);
            var director = Director(Level(Wave(0f, entry)));

            entry.Count = 50;
            director.Tick(0.1f);

            Assert.That(_data.Spawned, Has.Count.EqualTo(1));
        }

        private static void Tick(WaveDirector director, float delta, int times)
        {
            for (var i = 0; i < times; i++) director.Tick(delta);
        }
    }
}

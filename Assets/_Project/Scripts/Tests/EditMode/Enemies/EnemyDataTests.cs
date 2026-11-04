using System;
using System.Collections.Generic;
using Asteroids.Core.IConfigs;
using NUnit.Framework;

namespace Asteroids.Enemies.Tests
{
    [TestFixture]
    public sealed class EnemyDataTests
    {
        private sealed class FakeSpawner : IEnemySpawner
        {
            public FakeSpawner(string type, int alive = 0)
            {
                Type = type == null ? default : new EnemyTypeId(type);
                AliveCount = alive;
            }

            public EnemyTypeId Type { get; }
            public int AliveCount { get; set; }
            public int SpawnCount { get; private set; }
            public int DespawnAllCount { get; private set; }
            public Exception ThrowOnDespawnAll { get; set; }

            public void Spawn()
            {
                SpawnCount++;
                AliveCount++;
            }

            public void DespawnAll()
            {
                DespawnAllCount++;
                if (ThrowOnDespawnAll != null) throw ThrowOnDespawnAll;
                AliveCount = 0;
            }
        }

        [Test]
        public void Constructor_InvalidSpawners_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new EnemyData(null));
            Assert.Throws<ArgumentException>(() => new EnemyData(new IEnemySpawner[] { null }));
            Assert.Throws<ArgumentException>(() => new EnemyData(new IEnemySpawner[] { new FakeSpawner(null) }));
            Assert.Throws<ArgumentException>(() =>
                new EnemyData(new IEnemySpawner[] { new FakeSpawner("A"), new FakeSpawner("A") }));
        }

        [Test]
        public void AliveCount_IsTheSumOfEverySpawner()
        {
            var data = new EnemyData(new IEnemySpawner[] { new FakeSpawner("A", 2), new FakeSpawner("B", 3) });

            Assert.That(data.AliveCount, Is.EqualTo(5));
        }

        [Test]
        public void CanSpawn_OnlyKnownValidTypes()
        {
            var data = new EnemyData(new IEnemySpawner[] { new FakeSpawner("A") });

            Assert.That(data.CanSpawn(new EnemyTypeId("A")), Is.True);
            Assert.That(data.CanSpawn(new EnemyTypeId("B")), Is.False);
            Assert.That(data.CanSpawn(default), Is.False);
        }

        [Test]
        public void Spawn_GoesToTheSpawnerOfTheType()
        {
            var a = new FakeSpawner("A");
            var b = new FakeSpawner("B");
            var data = new EnemyData(new IEnemySpawner[] { a, b });

            data.Spawn(new EnemyTypeId("B"));

            Assert.That(a.SpawnCount, Is.Zero);
            Assert.That(b.SpawnCount, Is.EqualTo(1));
        }

        [Test]
        public void Spawn_UnknownType_Throws()
        {
            var data = new EnemyData(new IEnemySpawner[] { new FakeSpawner("A") });

            Assert.Throws<InvalidOperationException>(() => data.Spawn(new EnemyTypeId("B")));
            Assert.Throws<InvalidOperationException>(() => data.Spawn(default));
        }

        [Test]
        public void DespawnAll_DespawnsEveryType()
        {
            var spawners = new List<FakeSpawner> { new("A", 2), new("B", 1) };
            var data = new EnemyData(spawners);

            data.DespawnAll();

            Assert.That(data.AliveCount, Is.Zero);
            Assert.That(spawners.TrueForAll(spawner => spawner.DespawnAllCount == 1), Is.True);
        }

        [Test]
        public void DespawnAll_OneTypeThrows_TheOthersStillDespawnAndTheFailureIsRethrown()
        {
            var failing = new FakeSpawner("A", 2) { ThrowOnDespawnAll = new InvalidOperationException() };
            var other = new FakeSpawner("B", 1);
            var data = new EnemyData(new IEnemySpawner[] { failing, other });

            Assert.Throws<InvalidOperationException>(() => data.DespawnAll());

            Assert.That(other.AliveCount, Is.Zero);
        }
    }
}

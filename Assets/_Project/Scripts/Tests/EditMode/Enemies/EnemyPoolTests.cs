using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Asteroids.Enemies.Tests
{
    [TestFixture]
    public sealed class EnemyPoolTests
    {
        private sealed class FakeEnemy : IPooledEnemy<FakeEnemy>
        {
            public event Action<FakeEnemy> Finished;

            public bool IsInPlay { get; private set; }
            public int DisposeCount { get; private set; }
            public Exception ThrowOnDespawn { get; set; }

            public void Enter() => IsInPlay = true;

            public void Kill()
            {
                IsInPlay = false;
                Finished?.Invoke(this);
            }

            public void Despawn()
            {
                if (ThrowOnDespawn != null) throw ThrowOnDespawn;
                if (IsInPlay) Kill();
            }

            public void Dispose() => DisposeCount++;

            /// <summary>Raises <see cref="Finished"/> without being in play, as a faulty enemy might.</summary>
            public void RaiseFinished() => Finished?.Invoke(this);
        }

        private readonly List<FakeEnemy> _created = new();

        private EnemyPool<FakeEnemy> CreatePool(int maxIdle = 4) =>
            new(() =>
            {
                var enemy = new FakeEnemy();
                _created.Add(enemy);
                return enemy;
            }, maxIdle);

        [SetUp]
        public void SetUp() => _created.Clear();

        [Test]
        public void Constructor_InvalidArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new EnemyPool<FakeEnemy>(null, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnemyPool<FakeEnemy>(() => new FakeEnemy(), -1));
        }

        [Test]
        public void Launch_Null_Throws() =>
            Assert.Throws<ArgumentNullException>(() => CreatePool().Launch(null));

        [Test]
        public void Launch_PutsAnEnemyInPlayAndCountsIt()
        {
            var pool = CreatePool();

            pool.Launch(enemy => enemy.Enter());
            pool.Launch(enemy => enemy.Enter());

            Assert.That(pool.AliveCount, Is.EqualTo(2));
            Assert.That(_created.Count, Is.EqualTo(2));
            Assert.That(_created.TrueForAll(enemy => enemy.IsInPlay), Is.True);
        }

        [Test]
        public void Finished_CountsDownAndTheNextLaunchReusesTheEnemy()
        {
            var pool = CreatePool();
            pool.Launch(enemy => enemy.Enter());

            _created[0].Kill();
            pool.Launch(enemy => enemy.Enter());

            Assert.That(pool.AliveCount, Is.EqualTo(1));
            Assert.That(_created.Count, Is.EqualTo(1));
        }

        [Test]
        public void Finished_RaisedTwice_CountsDownOnce()
        {
            var pool = CreatePool();
            pool.Launch(enemy => enemy.Enter());
            pool.Launch(enemy => enemy.Enter());

            _created[0].Kill();
            Assert.DoesNotThrow(() => _created[0].RaiseFinished());

            Assert.That(pool.AliveCount, Is.EqualTo(1));
        }

        [Test]
        public void Launch_EnterThrows_ReturnsTheEnemyAndRethrows()
        {
            var pool = CreatePool();

            Assert.Throws<InvalidOperationException>(() => pool.Launch(_ => throw new InvalidOperationException()));
            pool.Launch(enemy => enemy.Enter());

            Assert.That(pool.AliveCount, Is.EqualTo(1));
            Assert.That(_created.Count, Is.EqualTo(1));
        }

        [Test]
        public void Launch_EnemyFinishesWhileEntering_IsNotCounted()
        {
            var pool = CreatePool();

            pool.Launch(enemy =>
            {
                enemy.Enter();
                enemy.Kill();
            });

            Assert.That(pool.AliveCount, Is.Zero);
        }

        [Test]
        public void Prewarm_CreatesIdleEnemiesThatLaunchesUse()
        {
            var pool = CreatePool();

            pool.Prewarm(2);
            pool.Launch(enemy => enemy.Enter());

            Assert.That(_created.Count, Is.EqualTo(2));
            Assert.That(pool.AliveCount, Is.EqualTo(1));
        }

        [Test]
        public void DespawnAll_TakesEveryEnemyOutOfPlayAndBackToThePool()
        {
            var pool = CreatePool();
            pool.Launch(enemy => enemy.Enter());
            pool.Launch(enemy => enemy.Enter());

            pool.DespawnAll();
            pool.Launch(enemy => enemy.Enter());
            pool.Launch(enemy => enemy.Enter());

            Assert.That(_created.Count, Is.EqualTo(2));
            Assert.That(pool.AliveCount, Is.EqualTo(2));
        }

        [Test]
        public void DespawnAll_LeavesNothingAlive()
        {
            var pool = CreatePool();
            pool.Launch(enemy => enemy.Enter());
            pool.Launch(enemy => enemy.Enter());

            pool.DespawnAll();

            Assert.That(pool.AliveCount, Is.Zero);
            Assert.That(_created.TrueForAll(enemy => !enemy.IsInPlay), Is.True);
        }

        [Test]
        public void DespawnAll_NothingInPlay_DoesNothing() => Assert.DoesNotThrow(() => CreatePool().DespawnAll());

        [Test]
        public void DespawnAll_OneThrows_TheRestStillLeaveAndTheFailureIsRethrown()
        {
            var pool = CreatePool();
            pool.Launch(enemy => enemy.Enter());
            pool.Launch(enemy => enemy.Enter());
            pool.Launch(enemy => enemy.Enter());
            _created[1].ThrowOnDespawn = new InvalidOperationException();

            Assert.Throws<InvalidOperationException>(() => pool.DespawnAll());

            Assert.That(_created[0].IsInPlay, Is.False);
            Assert.That(_created[2].IsInPlay, Is.False);
            Assert.That(pool.AliveCount, Is.EqualTo(1));
        }

        [Test]
        public void ReleasedBeyondMaxIdle_IsDisposed()
        {
            var pool = CreatePool(maxIdle: 1);
            pool.Launch(enemy => enemy.Enter());
            pool.Launch(enemy => enemy.Enter());

            pool.DespawnAll();

            Assert.That(_created.FindAll(enemy => enemy.DisposeCount == 1).Count, Is.EqualTo(1));
        }
    }
}

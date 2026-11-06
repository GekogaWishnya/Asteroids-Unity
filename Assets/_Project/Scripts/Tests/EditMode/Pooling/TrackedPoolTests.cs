using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Asteroids.Pooling.Tests
{
    /// <summary>
    /// <see cref="TrackedPool{T}"/>: spawning, counting what is in play, going back to the pool on
    /// <c>Finished</c>, and despawning everything. Enemies and projectiles rely on all of it.
    /// </summary>
    [TestFixture]
    public sealed class TrackedPoolTests
    {
        private sealed class FakeItem : IPooledObject<FakeItem>
        {
            public event Action<FakeItem> Finished;

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

            /// <summary>Raises <see cref="Finished"/> without being in play, as a faulty item might.</summary>
            public void RaiseFinished() => Finished?.Invoke(this);
        }

        private readonly List<FakeItem> _created = new();

        private TrackedPool<FakeItem> CreatePool(int maxIdle = 4) =>
            new(() =>
            {
                var item = new FakeItem();
                _created.Add(item);
                return item;
            }, maxIdle);

        [SetUp]
        public void SetUp() => _created.Clear();

        [Test]
        public void Constructor_InvalidArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new TrackedPool<FakeItem>(null, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TrackedPool<FakeItem>(() => new FakeItem(), -1));
        }

        [Test]
        public void Spawn_Null_Throws() =>
            Assert.Throws<ArgumentNullException>(() => CreatePool().Spawn(null));

        [Test]
        public void Spawn_PutsAnEnemyInPlayAndCountsIt()
        {
            var pool = CreatePool();

            pool.Spawn(item => item.Enter());
            pool.Spawn(item => item.Enter());

            Assert.That(pool.InPlayCount, Is.EqualTo(2));
            Assert.That(_created.Count, Is.EqualTo(2));
            Assert.That(_created.TrueForAll(item => item.IsInPlay), Is.True);
        }

        [Test]
        public void Finished_CountsDownAndTheNextSpawnReusesTheObject()
        {
            var pool = CreatePool();
            pool.Spawn(item => item.Enter());

            _created[0].Kill();
            pool.Spawn(item => item.Enter());

            Assert.That(pool.InPlayCount, Is.EqualTo(1));
            Assert.That(_created.Count, Is.EqualTo(1));
        }

        [Test]
        public void Finished_RaisedTwice_CountsDownOnce()
        {
            var pool = CreatePool();
            pool.Spawn(item => item.Enter());
            pool.Spawn(item => item.Enter());

            _created[0].Kill();
            Assert.DoesNotThrow(() => _created[0].RaiseFinished());

            Assert.That(pool.InPlayCount, Is.EqualTo(1));
        }

        [Test]
        public void Spawn_EnterThrows_ReturnsTheItemAndRethrows()
        {
            var pool = CreatePool();

            Assert.Throws<InvalidOperationException>(() => pool.Spawn(_ => throw new InvalidOperationException()));
            pool.Spawn(item => item.Enter());

            Assert.That(pool.InPlayCount, Is.EqualTo(1));
            Assert.That(_created.Count, Is.EqualTo(1));
        }

        [Test]
        public void Spawn_ItemFinishesWhileEntering_IsNotCounted()
        {
            var pool = CreatePool();

            pool.Spawn(item =>
            {
                item.Enter();
                item.Kill();
            });

            Assert.That(pool.InPlayCount, Is.Zero);
        }

        [Test]
        public void Prewarm_CreatesIdleObjectsThatSpawnsUse()
        {
            var pool = CreatePool();

            pool.Prewarm(2);
            pool.Spawn(item => item.Enter());

            Assert.That(_created.Count, Is.EqualTo(2));
            Assert.That(pool.InPlayCount, Is.EqualTo(1));
        }

        [Test]
        public void DespawnAll_TakesEveryItemOutOfPlayAndBackToThePool()
        {
            var pool = CreatePool();
            pool.Spawn(item => item.Enter());
            pool.Spawn(item => item.Enter());

            pool.DespawnAll();
            pool.Spawn(item => item.Enter());
            pool.Spawn(item => item.Enter());

            Assert.That(_created.Count, Is.EqualTo(2));
            Assert.That(pool.InPlayCount, Is.EqualTo(2));
        }

        [Test]
        public void DespawnAll_LeavesNothingAlive()
        {
            var pool = CreatePool();
            pool.Spawn(item => item.Enter());
            pool.Spawn(item => item.Enter());

            pool.DespawnAll();

            Assert.That(pool.InPlayCount, Is.Zero);
            Assert.That(_created.TrueForAll(item => !item.IsInPlay), Is.True);
        }

        [Test]
        public void DespawnAll_NothingInPlay_DoesNothing() => Assert.DoesNotThrow(() => CreatePool().DespawnAll());

        [Test]
        public void DespawnAll_OneThrows_TheRestStillLeaveAndTheFailureIsRethrown()
        {
            var pool = CreatePool();
            pool.Spawn(item => item.Enter());
            pool.Spawn(item => item.Enter());
            pool.Spawn(item => item.Enter());
            _created[1].ThrowOnDespawn = new InvalidOperationException();

            Assert.Throws<InvalidOperationException>(() => pool.DespawnAll());

            Assert.That(_created[0].IsInPlay, Is.False);
            Assert.That(_created[2].IsInPlay, Is.False);
            Assert.That(pool.InPlayCount, Is.EqualTo(1));
        }

        [Test]
        public void SpawnWithState_PassesTheStateToEnter()
        {
            var pool = CreatePool();
            var seen = 0;

            pool.Spawn(7, (item, state) =>
            {
                seen = state;
                item.Enter();
            });

            Assert.That(seen, Is.EqualTo(7));
            Assert.That(pool.InPlayCount, Is.EqualTo(1));
        }

        [Test]
        public void SpawnWithState_NullEnter_Throws() =>
            Assert.Throws<ArgumentNullException>(() => CreatePool().Spawn(1, null));

        [Test]
        public void Spawn_FactoryReturnsNull_Throws()
        {
            var pool = new TrackedPool<FakeItem>(() => null, 1);

            Assert.Throws<InvalidOperationException>(() => pool.Spawn(item => item.Enter()));
            Assert.That(pool.InPlayCount, Is.Zero);
        }

        [Test]
        public void ReleasedBeyondMaxIdle_IsDisposed()
        {
            var pool = CreatePool(maxIdle: 1);
            pool.Spawn(item => item.Enter());
            pool.Spawn(item => item.Enter());

            pool.DespawnAll();

            Assert.That(_created.FindAll(item => item.DisposeCount == 1).Count, Is.EqualTo(1));
        }
    }
}

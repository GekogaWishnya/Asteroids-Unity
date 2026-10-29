using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Asteroids.Common.Tests
{
    [TestFixture]
    public sealed class ObjectPoolTests
    {
        private sealed class Item { }

        private sealed class Recorder
        {
            public readonly List<Item> Created = new();
            public readonly List<Item> Got = new();
            public readonly List<Item> Released = new();
            public readonly List<Item> Destroyed = new();

            public ObjectPool<Item> CreatePool(int maxIdle = int.MaxValue) =>
                new(Create, Got.Add, Released.Add, Destroyed.Add, maxIdle);

            private Item Create()
            {
                var item = new Item();
                Created.Add(item);
                return item;
            }
        }

        [Test]
        public void Constructor_NullCreate_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ObjectPool<Item>(null));
        }

        [Test]
        public void Constructor_NegativeMaxIdle_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ObjectPool<Item>(() => new Item(), maxIdle: -1));
        }

        [Test]
        public void Get_EmptyPool_CreatesItemAndRunsOnGet()
        {
            var recorder = new Recorder();
            var pool = recorder.CreatePool();

            var item = pool.Get();

            Assert.That(recorder.Created, Is.EqualTo(new[] { item }));
            Assert.That(recorder.Got, Is.EqualTo(new[] { item }));
            Assert.That(pool.CountActive, Is.EqualTo(1));
            Assert.That(pool.CountIdle, Is.EqualTo(0));
        }

        [Test]
        public void Get_AfterRelease_ReusesTheSameItem()
        {
            var recorder = new Recorder();
            var pool = recorder.CreatePool();
            var first = pool.Get();
            pool.Release(first);

            var second = pool.Get();

            Assert.That(second, Is.SameAs(first));
            Assert.That(recorder.Created, Has.Count.EqualTo(1));
            Assert.That(recorder.Got, Has.Count.EqualTo(2));
        }

        [Test]
        public void Get_ManyWithoutRelease_NeverRefuses()
        {
            var recorder = new Recorder();
            var pool = recorder.CreatePool(maxIdle: 1);

            for (var i = 0; i < 100; i++) pool.Get();

            Assert.That(recorder.Created, Has.Count.EqualTo(100));
            Assert.That(pool.CountActive, Is.EqualTo(100));
        }

        [Test]
        public void Get_CreateReturnsNull_Throws()
        {
            var pool = new ObjectPool<Item>(() => null);

            Assert.Throws<InvalidOperationException>(() => pool.Get());
            Assert.That(pool.CountActive, Is.EqualTo(0));
        }

        [Test]
        public void Get_OnGetThrows_DestroysItemAndRethrows()
        {
            var destroyed = new List<Item>();
            var pool = new ObjectPool<Item>(
                () => new Item(),
                onGet: _ => throw new InvalidOperationException("boom"),
                onDestroy: destroyed.Add);

            Assert.Throws<InvalidOperationException>(() => pool.Get());

            Assert.That(destroyed, Has.Count.EqualTo(1));
            Assert.That(pool.CountActive, Is.EqualTo(0));
            Assert.That(pool.CountIdle, Is.EqualTo(0));
        }

        [Test]
        public void Release_RunsOnReleaseAndKeepsItemIdle()
        {
            var recorder = new Recorder();
            var pool = recorder.CreatePool();
            var item = pool.Get();

            pool.Release(item);

            Assert.That(recorder.Released, Is.EqualTo(new[] { item }));
            Assert.That(pool.CountActive, Is.EqualTo(0));
            Assert.That(pool.CountIdle, Is.EqualTo(1));
            Assert.That(recorder.Destroyed, Is.Empty);
        }

        [Test]
        public void Release_AboveMaxIdle_DestroysTheSurplus()
        {
            var recorder = new Recorder();
            var pool = recorder.CreatePool(maxIdle: 1);
            var first = pool.Get();
            var second = pool.Get();

            pool.Release(first);
            pool.Release(second);

            Assert.That(pool.CountIdle, Is.EqualTo(1));
            Assert.That(recorder.Destroyed, Is.EqualTo(new[] { second }));
        }

        [Test]
        public void Release_MaxIdleZero_AlwaysDestroys()
        {
            var recorder = new Recorder();
            var pool = recorder.CreatePool(maxIdle: 0);
            var item = pool.Get();

            pool.Release(item);

            Assert.That(pool.CountIdle, Is.EqualTo(0));
            Assert.That(recorder.Destroyed, Is.EqualTo(new[] { item }));
        }

        [Test]
        public void Release_Twice_Throws()
        {
            var recorder = new Recorder();
            var pool = recorder.CreatePool();
            var item = pool.Get();
            pool.Release(item);

            Assert.Throws<InvalidOperationException>(() => pool.Release(item));
        }

        [Test]
        public void Release_ItemFromElsewhere_Throws()
        {
            var pool = new Recorder().CreatePool();

            Assert.Throws<InvalidOperationException>(() => pool.Release(new Item()));
        }

        [Test]
        public void Release_Null_Throws()
        {
            var pool = new Recorder().CreatePool();

            Assert.Throws<ArgumentNullException>(() => pool.Release(null));
        }

        [Test]
        public void Release_OnReleaseThrows_DestroysItemAndRethrows()
        {
            var destroyed = new List<Item>();
            var pool = new ObjectPool<Item>(
                () => new Item(),
                onRelease: _ => throw new InvalidOperationException("boom"),
                onDestroy: destroyed.Add);
            var item = pool.Get();

            Assert.Throws<InvalidOperationException>(() => pool.Release(item));

            Assert.That(destroyed, Is.EqualTo(new[] { item }));
            Assert.That(pool.CountActive, Is.EqualTo(0));
            Assert.That(pool.CountIdle, Is.EqualTo(0));
        }

        [Test]
        public void Prewarm_CreatesIdleItemsInReleasedState()
        {
            var recorder = new Recorder();
            var pool = recorder.CreatePool();

            pool.Prewarm(3);

            Assert.That(pool.CountIdle, Is.EqualTo(3));
            Assert.That(pool.CountActive, Is.EqualTo(0));
            Assert.That(recorder.Released, Has.Count.EqualTo(3));
            Assert.That(recorder.Got, Is.Empty);
        }

        [Test]
        public void Prewarm_AboveMaxIdle_StopsAtMaxIdleWithoutDestroying()
        {
            var recorder = new Recorder();
            var pool = recorder.CreatePool(maxIdle: 2);

            pool.Prewarm(5);

            Assert.That(recorder.Created, Has.Count.EqualTo(2));
            Assert.That(pool.CountIdle, Is.EqualTo(2));
            Assert.That(recorder.Destroyed, Is.Empty);
        }

        [Test]
        public void Prewarm_ThenGet_UsesPrewarmedItem()
        {
            var recorder = new Recorder();
            var pool = recorder.CreatePool();
            pool.Prewarm(2);

            pool.Get();

            Assert.That(recorder.Created, Has.Count.EqualTo(2));
            Assert.That(pool.CountIdle, Is.EqualTo(1));
        }

        [Test]
        public void Prewarm_WhenEnoughAreIdle_CreatesNothing()
        {
            var recorder = new Recorder();
            var pool = recorder.CreatePool();
            pool.Prewarm(2);

            pool.Prewarm(2);

            Assert.That(recorder.Created, Has.Count.EqualTo(2));
        }

        [Test]
        public void Prewarm_Negative_Throws()
        {
            var pool = new Recorder().CreatePool();

            Assert.Throws<ArgumentOutOfRangeException>(() => pool.Prewarm(-1));
        }

        [Test]
        public void Clear_DestroysIdleItemsOnly()
        {
            var recorder = new Recorder();
            var pool = recorder.CreatePool();
            var kept = pool.Get();
            var returned = pool.Get();
            pool.Release(returned);

            pool.Clear();

            Assert.That(recorder.Destroyed, Is.EqualTo(new[] { returned }));
            Assert.That(pool.CountIdle, Is.EqualTo(0));
            Assert.That(pool.CountActive, Is.EqualTo(1));
            Assert.DoesNotThrow(() => pool.Release(kept));
        }

        [Test]
        public void Clear_DestroyThrows_StillDestroysTheRestAndRethrows()
        {
            var attempts = 0;
            var pool = new ObjectPool<Item>(
                () => new Item(),
                onDestroy: _ =>
                {
                    attempts++;
                    throw new InvalidOperationException("boom");
                });
            pool.Prewarm(3);

            Assert.Throws<InvalidOperationException>(() => pool.Clear());

            Assert.That(attempts, Is.EqualTo(3));
            Assert.That(pool.CountIdle, Is.EqualTo(0));
        }
    }
}

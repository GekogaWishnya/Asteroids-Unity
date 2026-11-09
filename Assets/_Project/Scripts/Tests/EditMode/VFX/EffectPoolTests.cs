using System;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.VFX.Tests
{
    [TestFixture]
    public sealed class EffectPoolTests
    {
        private TestEffectConfig _config;
        private FakeEffectFactory _factory;

        [SetUp]
        public void SetUp()
        {
            _config = new TestEffectConfig { Scale = 2f, PoolPrewarm = 2, PoolMaxIdle = 4, MaxActive = 3 };
            _factory = new FakeEffectFactory();
        }

        private EffectPool CreatePool() => new(_config, _factory);

        [Test]
        public void Constructor_InvalidArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new EffectPool(null, _factory));
            Assert.Throws<ArgumentNullException>(() => new EffectPool(_config, null));
            Assert.Throws<InvalidOperationException>(() =>
                new EffectPool(new TestEffectConfig { MaxActive = 0 }, _factory));
        }

        [Test]
        public void Prewarm_CreatesIdleObjects()
        {
            var pool = CreatePool();

            pool.Prewarm();

            Assert.That(_factory.Created, Has.Count.EqualTo(2));
            Assert.That(pool.PlayingCount, Is.EqualTo(0));
        }

        [Test]
        public void Play_PlaysAtThePositionWithTheConfiguredScale()
        {
            var pool = CreatePool();

            pool.Play(new Vector2(1f, 2f));

            var effect = _factory.Created[0];
            Assert.That(effect.IsPlaying, Is.True);
            Assert.That(effect.Position, Is.EqualTo(new Vector2(1f, 2f)));
            Assert.That(effect.Scale, Is.EqualTo(2f));
            Assert.That(pool.PlayingCount, Is.EqualTo(1));
        }

        [Test]
        public void Finished_ReturnsTheObjectForReuse()
        {
            var pool = CreatePool();
            pool.Play(Vector2.zero);
            var first = _factory.Created[0];

            first.Finish();
            pool.Play(Vector2.one);

            Assert.That(_factory.Created, Has.Count.EqualTo(1));
            Assert.That(first.PlayCount, Is.EqualTo(2));
            Assert.That(pool.PlayingCount, Is.EqualTo(1));
        }

        [Test]
        public void Finished_Twice_ReleasesOnlyOnce()
        {
            var pool = CreatePool();
            pool.Play(Vector2.zero);
            var effect = _factory.Created[0];

            effect.Finish();

            Assert.DoesNotThrow(effect.Finish);
            Assert.That(pool.PlayingCount, Is.EqualTo(0));
        }

        [Test]
        public void Play_AtMaxActive_RecyclesTheOldest()
        {
            var pool = CreatePool();
            pool.Play(new Vector2(1f, 0f));
            pool.Play(new Vector2(2f, 0f));
            pool.Play(new Vector2(3f, 0f));
            var oldest = _factory.Created[0];

            pool.Play(new Vector2(4f, 0f));

            Assert.That(pool.PlayingCount, Is.EqualTo(3));
            Assert.That(_factory.Created, Has.Count.EqualTo(3));
            Assert.That(oldest.Position, Is.EqualTo(new Vector2(4f, 0f)));
            Assert.That(oldest.StopCount, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void Play_WhenObjectFails_ReturnsItAndRethrows()
        {
            var pool = CreatePool();
            pool.Prewarm();
            _factory.Created.ForEach(effect => effect.FailOnPlay = true);

            Assert.Throws<InvalidOperationException>(() => pool.Play(Vector2.zero));
            Assert.That(pool.PlayingCount, Is.EqualTo(0));
        }

        [Test]
        public void SetPaused_FreezesPlayingAndNewEffects()
        {
            var pool = CreatePool();
            pool.Play(Vector2.zero);

            pool.SetPaused(true);
            pool.Play(Vector2.one);

            Assert.That(_factory.Created.TrueForAll(effect => effect.IsPaused), Is.True);

            pool.SetPaused(false);

            Assert.That(_factory.Created.TrueForAll(effect => !effect.IsPaused), Is.True);
        }

        [Test]
        public void Dispose_DestroysPlayingAndIdleObjects()
        {
            var pool = CreatePool();
            pool.Prewarm();
            pool.Play(Vector2.zero);
            pool.Play(Vector2.one);
            pool.Play(Vector2.up);

            pool.Dispose();

            Assert.That(_factory.Created.TrueForAll(effect => effect.IsDisposed), Is.True);
            Assert.That(pool.PlayingCount, Is.EqualTo(0));
        }

        [Test]
        public void Dispose_ThenFinishOrPlay_IsSafeOrRefused()
        {
            var pool = CreatePool();
            pool.Play(Vector2.zero);
            var effect = _factory.Created[0];

            pool.Dispose();

            Assert.DoesNotThrow(effect.Finish);
            Assert.Throws<ObjectDisposedException>(() => pool.Play(Vector2.zero));
        }
    }
}
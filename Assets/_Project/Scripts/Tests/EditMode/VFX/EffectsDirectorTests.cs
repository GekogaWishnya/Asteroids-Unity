using System;
using System.Text.RegularExpressions;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Asteroids.VFX.Tests
{
    [TestFixture]
    public sealed class EffectsDirectorTests
    {
        private static readonly EnemyTypeId Asteroid = new("Asteroid");

        private TestEffectsConfig _config;
        private TestEffectConfig _shipHit;
        private TestEffectConfig _shipDeath;
        private TestEffectConfig _asteroidHit;
        private TestEffectConfig _asteroidDeath;
        private FakeEffectFactory _factory;
        private RecordingErrorLog _errors;

        [SetUp]
        public void SetUp()
        {
            _shipHit = new TestEffectConfig("ShipHit");
            _shipDeath = new TestEffectConfig("ShipDeath");
            _asteroidHit = new TestEffectConfig("AsteroidHit");
            _asteroidDeath = new TestEffectConfig("AsteroidDeath") { PoolPrewarm = 3 };
            _config = new TestEffectsConfig { Ship = new TestFeedback { Hit = _shipHit, Death = _shipDeath } };
            _config.EnemyList.Add(new TestEnemyFeedback("Asteroid") { Hit = _asteroidHit, Death = _asteroidDeath });
            _factory = new FakeEffectFactory();
            _errors = new RecordingErrorLog();
        }

        private EffectsDirector CreateDirector() => new(_config, _factory, _errors);

        [Test]
        public void Constructor_InvalidArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new EffectsDirector(null, _factory, _errors));
            Assert.Throws<ArgumentNullException>(() => new EffectsDirector(_config, null, _errors));
            Assert.Throws<ArgumentNullException>(() => new EffectsDirector(_config, _factory, null));

            _config.Ship = new TestFeedback { Death = new TestEffectConfig("Broken") { Scale = 0f } };
            Assert.Throws<InvalidOperationException>(() => CreateDirector());
        }

        [Test]
        public void Initialize_PrewarmsEveryPool()
        {
            var director = CreateDirector();

            director.Initialize();

            Assert.That(_factory.Created.FindAll(e => e.Config == _asteroidDeath), Has.Count.EqualTo(3));
        }

        [Test]
        public void ShipDamaged_PlaysTheShipHitWhereTheShipWas()
        {
            var director = CreateDirector();

            director.OnShipDamaged(new ShipDamaged(new Vector2(1f, 2f), false));

            var playing = _factory.Playing(_shipHit);
            Assert.That(playing, Has.Count.EqualTo(1));
            Assert.That(playing[0].Position, Is.EqualTo(new Vector2(1f, 2f)));
        }

        [Test]
        public void ShipDamaged_Fatal_SkipsTheHitUnlessAskedFor()
        {
            var director = CreateDirector();

            director.OnShipDamaged(new ShipDamaged(Vector2.zero, true));
            Assert.That(_factory.Playing(_shipHit), Is.Empty);

            ((TestFeedback)_config.Ship).PlayHitOnFatal = true;
            director = CreateDirector();
            director.OnShipDamaged(new ShipDamaged(Vector2.zero, true));
            Assert.That(_factory.Playing(_shipHit), Has.Count.EqualTo(1));
        }

        [Test]
        public void ShipDestroyed_PlaysTheShipDeathAtTheShipsPose()
        {
            var director = CreateDirector();

            director.OnShipDestroyed(new ShipDestroyed(new TestEntity(new Vector2(5f, 6f))));

            var playing = _factory.Playing(_shipDeath);
            Assert.That(playing, Has.Count.EqualTo(1));
            Assert.That(playing[0].Position, Is.EqualTo(new Vector2(5f, 6f)));
        }

        [Test]
        public void EnemyEvents_PlayTheEffectsOfTheirType()
        {
            var director = CreateDirector();

            director.OnEnemyDamaged(new EnemyDamaged(Asteroid, Vector2.one, false));
            director.OnEnemyDamaged(new EnemyDamaged(Asteroid, Vector2.one, true));
            director.OnEnemyDied(new EnemyDied(Asteroid, Vector2.one));

            Assert.That(_factory.Playing(_asteroidHit), Has.Count.EqualTo(1));
            Assert.That(_factory.Playing(_asteroidDeath), Has.Count.EqualTo(1));
        }

        [Test]
        public void EntriesWithoutHitEffect_ShowNothingOnHits()
        {
            _config.Ship = new TestFeedback { Death = _shipDeath };
            var director = CreateDirector();

            director.OnShipDamaged(new ShipDamaged(Vector2.zero, false));

            Assert.That(_factory.Created, Is.Empty);
        }

        [Test]
        public void EntriesWithoutDeathEffect_ShowNothingOnDeath()
        {
            // Death is as optional as Hit: an entry with neither is valid and simply draws nothing.
            _config.Ship = new TestFeedback { Death = null };
            _config.EnemyList.Clear();
            _config.EnemyList.Add(new TestEnemyFeedback("Asteroid") { Death = null });
            var director = CreateDirector();

            director.OnEnemyDied(new EnemyDied(new EnemyTypeId("Asteroid"), Vector2.zero));
            director.OnEnemyDamaged(new EnemyDamaged(new EnemyTypeId("Asteroid"), Vector2.zero, true));

            Assert.That(_factory.Created, Is.Empty);
            Assert.That(_errors.Reports, Is.Empty);
        }

        [Test]
        public void UnknownEnemyType_WarnsOnceAndPlaysNothing()
        {
            var director = CreateDirector();
            var unknown = new EnemyTypeId("Comet");
            LogAssert.Expect(LogType.Warning, new Regex("Comet"));

            director.OnEnemyDied(new EnemyDied(unknown, Vector2.zero));
            director.OnEnemyDied(new EnemyDied(unknown, Vector2.zero));

            Assert.That(_factory.Created, Is.Empty);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void FailingEffect_IsReportedAndNeverReachesTheCaller()
        {
            _factory.FailOnCreate = true;
            var director = CreateDirector();

            Assert.DoesNotThrow(() => director.OnEnemyDied(new EnemyDied(Asteroid, Vector2.zero)));

            Assert.That(_errors.Reports, Has.Count.EqualTo(1));
            Assert.That(_errors.Reports[0].Failure.Message, Does.Contain("broken prefab"));
            Assert.That(_errors.Reports[0].Context, Does.Contain("Asteroid"));
        }

        [Test]
        public void SharedEffectConfig_UsesOnePool()
        {
            _config.EnemyList.Add(new TestEnemyFeedback("Ufo") { Death = _asteroidDeath });
            var director = CreateDirector();

            director.Initialize();

            Assert.That(_factory.Created.FindAll(e => e.Config == _asteroidDeath), Has.Count.EqualTo(3));
        }

        [Test]
        public void SetPaused_PausesPlayingAndLaterEffects()
        {
            var director = CreateDirector();
            director.OnEnemyDied(new EnemyDied(Asteroid, Vector2.zero));

            director.SetPaused(true);
            director.OnShipDamaged(new ShipDamaged(Vector2.zero, false));

            Assert.That(director.IsPaused, Is.True);
            Assert.That(_factory.Created.TrueForAll(e => e.IsPaused), Is.True);

            director.SetPaused(false);

            Assert.That(_factory.Created.TrueForAll(e => !e.IsPaused), Is.True);
        }

        [Test]
        public void Dispose_DestroysEverythingAndIgnoresLaterEvents()
        {
            var director = CreateDirector();
            director.Initialize();
            director.OnEnemyDied(new EnemyDied(Asteroid, Vector2.zero));
            var created = _factory.Created.Count;

            director.Dispose();
            director.OnEnemyDied(new EnemyDied(Asteroid, Vector2.zero));

            Assert.That(_factory.Created, Has.Count.EqualTo(created));
            Assert.That(_factory.Created.TrueForAll(e => e.IsDisposed), Is.True);
        }
    }
}
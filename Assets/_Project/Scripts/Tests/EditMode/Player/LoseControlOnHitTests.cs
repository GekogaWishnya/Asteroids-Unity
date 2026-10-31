using System;
using System.Collections.Generic;
using Asteroids.Core;
using Asteroids.Physics;
using NUnit.Framework;

namespace Asteroids.Player.Tests
{
    [TestFixture]
    public sealed class LoseControlOnHitTests
    {
        private sealed class RecordingDamageable : IDamageable
        {
            private readonly Func<bool> _isLocked;

            public RecordingDamageable(Func<bool> isLocked) => _isLocked = isLocked;

            public List<int> Hits { get; } = new();
            public bool LockedWhenHit { get; private set; }

            public void TakeDamage(int amount)
            {
                LockedWhenHit = _isLocked();
                Hits.Add(amount);
            }
        }

        private TestCombatConfig _config;
        private ShipControlLoss _loss;
        private RecordingDamageable _inner;
        private LoseControlOnHit _damageable;

        [SetUp]
        public void SetUp()
        {
            _config = new TestCombatConfig { ControlLossSeconds = 3f };
            _loss = new ShipControlLoss(new FakeSimulationLoop());
            _loss.Attach(new Entity(new BodyPose()));
            _inner = new RecordingDamageable(() => _loss.IsLocked);
            _damageable = new LoseControlOnHit(_inner, _loss, _config);
        }

        [Test]
        public void TakeDamage_PassesTheDamageOn()
        {
            _damageable.TakeDamage(2);

            Assert.That(_inner.Hits, Is.EqualTo(new[] { 2 }));
        }

        [Test]
        public void TakeDamage_LocksTheControlsForTheConfiguredDuration()
        {
            _damageable.TakeDamage(1);

            Assert.That(_loss.Remaining, Is.EqualTo(3f));
        }

        [Test]
        public void TakeDamage_LocksBeforeTheDamageIsApplied()
        {
            _damageable.TakeDamage(1);

            Assert.That(_inner.LockedWhenHit, Is.True);
        }

        [Test]
        public void TakeDamage_ReadsTheDurationOnEveryHit()
        {
            _config.ControlLossSeconds = 5f;

            _damageable.TakeDamage(1);

            Assert.That(_loss.Remaining, Is.EqualTo(5f));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void TakeDamage_NotPositive_ThrowsWithoutLocking(int amount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _damageable.TakeDamage(amount));
            Assert.That(_loss.IsLocked, Is.False);
            Assert.That(_inner.Hits, Is.Empty);
        }

        [Test]
        public void Construct_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new LoseControlOnHit(null, _loss, _config));
            Assert.Throws<ArgumentNullException>(() => new LoseControlOnHit(_inner, null, _config));
            Assert.Throws<ArgumentNullException>(() => new LoseControlOnHit(_inner, _loss, null));
        }
    }
}

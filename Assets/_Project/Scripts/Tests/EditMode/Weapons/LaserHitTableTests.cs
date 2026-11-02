using System;
using Asteroids.Weapons.Laser;
using NUnit.Framework;

namespace Asteroids.Weapons.Tests
{
    [TestFixture]
    public sealed class LaserHitTableTests
    {
        private TestLaserConfig _config;
        private FakeDamageLookup _damage;
        private LaserHitTable _table;
        private TestEntity _target;
        private RecordingDamageable _health;

        [SetUp]
        public void SetUp()
        {
            _config = new TestLaserConfig { HitDamage = 2, HitInterval = 0.5f };
            _damage = new FakeDamageLookup();
            _table = new LaserHitTable(_config, _damage);
            _target = new TestEntity();
            _health = new RecordingDamageable();
            _damage.Add(_target, _health);
        }

        [Test]
        public void Tick_AfterEntering_HitsOnTheFirstTick()
        {
            _table.Enter(_target);

            _table.Tick(0.01f);

            Assert.That(_health.Hits, Is.EqualTo(new[] { 2 }));
        }

        [Test]
        public void Tick_TargetStays_HitsAgainEveryInterval()
        {
            _table.Enter(_target);
            _table.Tick(0.01f);

            _table.Tick(0.25f);
            _table.Tick(0.125f);
            Assert.That(_health.Hits.Count, Is.EqualTo(1), "before the interval");

            _table.Tick(0.125f);
            Assert.That(_health.Hits.Count, Is.EqualTo(2), "on the interval");
        }

        [Test]
        public void Tick_LongTick_HitsAtMostOncePerTick()
        {
            _table.Enter(_target);
            _table.Tick(0.01f);

            _table.Tick(2f);
            Assert.That(_health.Hits.Count, Is.EqualTo(2));

            _table.Tick(0.01f);
            Assert.That(_health.Hits.Count, Is.EqualTo(3), "the overshoot is carried, up to one interval");
        }

        [Test]
        public void Tick_EveryTargetHasATimerOfItsOwn()
        {
            var late = new TestEntity();
            var lateHealth = new RecordingDamageable();
            _damage.Add(late, lateHealth);
            _table.Enter(_target);
            _table.Tick(0.3f);

            _table.Enter(late);
            _table.Tick(0.3f);
            _table.Tick(0.3f);

            Assert.That(_health.Hits.Count, Is.EqualTo(2), "early target: on contact and 0.6 s later");
            Assert.That(lateHealth.Hits.Count, Is.EqualTo(1), "late target: on contact, 0.3 s is not yet an interval");
        }

        [Test]
        public void Exit_LastContact_StopsTheHits()
        {
            _table.Enter(_target);
            _table.Tick(0.01f);

            _table.Exit(_target);
            _table.Tick(1f);

            Assert.That(_table.Contains(_target), Is.False);
            Assert.That(_health.Hits.Count, Is.EqualTo(1));
        }

        [Test]
        public void Exit_TargetWithTwoColliders_StaysUntilBothLeave()
        {
            _table.Enter(_target);
            _table.Enter(_target);

            _table.Exit(_target);
            Assert.That(_table.Contains(_target), Is.True);

            _table.Exit(_target);
            Assert.That(_table.Contains(_target), Is.False);
        }

        [Test]
        public void Enter_SecondCollider_DoesNotRestartTheTimer()
        {
            _table.Enter(_target);
            _table.Tick(0.01f);

            _table.Enter(_target);
            _table.Tick(0.01f);

            Assert.That(_health.Hits.Count, Is.EqualTo(1));
        }

        [Test]
        public void Enter_AfterLeaving_HitsOnContactAgain()
        {
            _table.Enter(_target);
            _table.Tick(0.01f);
            _table.Exit(_target);

            _table.Enter(_target);
            _table.Tick(0.01f);

            Assert.That(_health.Hits.Count, Is.EqualTo(2));
        }

        [Test]
        public void Exit_UnknownTarget_IsIgnored()
        {
            Assert.DoesNotThrow(() => _table.Exit(new TestEntity()));
            Assert.That(_table.Count, Is.EqualTo(0));
        }

        [Test]
        public void Tick_TargetThatCannotBeHurt_IsKeptAndNotHit()
        {
            var wall = new TestEntity();
            _table.Enter(wall);

            Assert.DoesNotThrow(() => _table.Tick(0.01f));
            Assert.That(_table.Contains(wall), Is.True);
        }

        [Test]
        public void Tick_AHitKillsItsTarget_TheOtherTargetsAreStillHit()
        {
            var other = new TestEntity();
            var otherHealth = new RecordingDamageable();
            _damage.Add(other, otherHealth);
            _table.Enter(_target);
            _table.Enter(other);
            // Dying makes the colliders leave play, which reports an exit at once.
            _health.OnHit = () => _table.Exit(_target);
            otherHealth.OnHit = () => _table.Exit(other);

            _table.Tick(0.01f);

            Assert.That(_health.Hits.Count, Is.EqualTo(1));
            Assert.That(otherHealth.Hits.Count, Is.EqualTo(1));
            Assert.That(_table.Count, Is.EqualTo(0));
        }

        [Test]
        public void Tick_ATargetEntersDuringTheTick_IsHitOnTheNextOne()
        {
            var fragment = new TestEntity();
            var fragmentHealth = new RecordingDamageable();
            _damage.Add(fragment, fragmentHealth);
            _table.Enter(_target);
            _health.OnHit = () => _table.Enter(fragment);

            _table.Tick(0.01f);
            Assert.That(fragmentHealth.Hits, Is.Empty);

            _table.Tick(0.01f);
            Assert.That(fragmentHealth.Hits.Count, Is.EqualTo(1));
        }

        [Test]
        public void Clear_ForgetsEveryTarget()
        {
            _table.Enter(_target);
            _table.Enter(new TestEntity());

            _table.Clear();
            _table.Tick(1f);

            Assert.That(_table.Count, Is.EqualTo(0));
            Assert.That(_health.Hits, Is.Empty);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        public void Tick_InvalidHitInterval_Throws(float interval)
        {
            _config.HitInterval = interval;

            Assert.Throws<InvalidOperationException>(() => _table.Tick(0.1f));
        }

        [Test]
        public void EnterAndExit_Null_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => _table.Enter(null));
            Assert.Throws<ArgumentNullException>(() => _table.Exit(null));
        }

        [Test]
        public void Construct_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new LaserHitTable(null, _damage));
            Assert.Throws<ArgumentNullException>(() => new LaserHitTable(_config, null));
        }
    }
}

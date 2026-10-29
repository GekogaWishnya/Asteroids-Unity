using System;
using Asteroids.Core;
using Asteroids.Invulnerability;
using NUnit.Framework;

namespace Asteroids.Player.Tests
{
    [TestFixture]
    public sealed class WaveStartProtectionTests
    {
        private sealed class NullDamageable : IDamageable
        {
            public void TakeDamage(int amount) { }
        }

        private static readonly WaveStarted Signal = new(1, 3);

        private TestCombatConfig _config;
        private WaveStartProtection _protection;
        private InvulnerabilityGuard _guard;

        [SetUp]
        public void SetUp()
        {
            _config = new TestCombatConfig { WaveStartProtectionSeconds = 2f };
            _protection = new WaveStartProtection(_config);
            _guard = NewGuard();
        }

        private InvulnerabilityGuard NewGuard() =>
            new(new NullDamageable(), _config, new FakeProtectionTargets());

        [Test]
        public void Constructor_NullConfig_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new WaveStartProtection(null));
        }

        [Test]
        public void Attach_NullGuard_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _protection.Attach(null));
        }

        [Test]
        public void OnWaveStarted_WithAnAttachedGuard_ProtectsIt()
        {
            _protection.Attach(_guard);

            _protection.OnWaveStarted(Signal);

            Assert.That(_guard.IsActive, Is.True);
        }

        [Test]
        public void OnWaveStarted_WithoutAGuard_DoesNothing()
        {
            Assert.DoesNotThrow(() => _protection.OnWaveStarted(Signal));
        }

        [Test]
        public void OnWaveStarted_ZeroSeconds_DoesNothing()
        {
            _config.WaveStartProtectionSeconds = 0f;
            _protection.Attach(_guard);

            _protection.OnWaveStarted(Signal);

            Assert.That(_guard.IsActive, Is.False);
        }

        [Test]
        public void OnWaveStarted_AfterTheGuardWasDetached_DoesNothing()
        {
            _protection.Attach(_guard).Dispose();

            _protection.OnWaveStarted(Signal);

            Assert.That(_guard.IsActive, Is.False);
        }

        [Test]
        public void OnWaveStarted_ReadsTheDurationFromTheConfigEachTime()
        {
            var loop = new FakeSimulationLoop();
            _guard.Attach(loop);
            _protection.Attach(_guard);
            _protection.OnWaveStarted(Signal);
            loop.Tick(2f);
            Assert.That(_guard.IsActive, Is.False);

            _config.WaveStartProtectionSeconds = 5f;
            _protection.OnWaveStarted(new WaveStarted(2, 3));
            loop.Tick(4.9f);

            Assert.That(_guard.IsActive, Is.True);
        }

        [Test]
        public void Attach_WhileAnotherGuardIsAttached_Throws()
        {
            _protection.Attach(_guard);

            Assert.Throws<InvalidOperationException>(() => _protection.Attach(NewGuard()));
        }

        [Test]
        public void Attach_AfterDispose_Works()
        {
            _protection.Attach(_guard).Dispose();

            Assert.DoesNotThrow(() => _protection.Attach(NewGuard()));
        }

        [Test]
        public void Dispose_Twice_DoesNotDetachTheNewGuard()
        {
            var first = _protection.Attach(_guard);
            first.Dispose();
            var second = NewGuard();
            _protection.Attach(second);

            first.Dispose();
            _protection.OnWaveStarted(Signal);

            Assert.That(second.IsActive, Is.True);
        }
    }
}

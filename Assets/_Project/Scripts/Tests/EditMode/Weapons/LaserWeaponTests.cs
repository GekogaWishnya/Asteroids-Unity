using System;
using Asteroids.Core;
using Asteroids.Weapons.Laser;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Weapons.Tests
{
    [TestFixture]
    public sealed class LaserWeaponTests
    {
        private const float Tolerance = 1e-4f;

        private TestLaserConfig _config;
        private FakeSimulationLoop _loop;
        private FakeBeamView _beam;
        private LaserBeamRegistry _beams;
        private FakeDamageLookup _damage;
        private TestEntity _carrier;
        private TestTrigger _trigger;

        [SetUp]
        public void SetUp()
        {
            // Refills 10/s with no delay at first, a press needs 20, the beam drains 50/s.
            _config = new TestLaserConfig
            {
                MaxEnergy = 100f, DrainPerSecond = 50f, RefillPerSecond = 10f, RefillDelay = 1f,
                MinEnergyToFire = 20f, HitDamage = 3, HitInterval = 0.5f, MuzzleOffset = 0.5f
            };
            _loop = new FakeSimulationLoop();
            _beam = new FakeBeamView();
            _beams = new LaserBeamRegistry();
            _damage = new FakeDamageLookup();
            _carrier = new TestEntity();
            _trigger = new TestTrigger();
        }

        private LaserWeapon Create() => new(_config, _loop, _beam, _beams, _damage);

        /// <summary>Attaches the weapon and lets the tank fill up to <paramref name="energy"/>.</summary>
        private LaserWeapon CreateCharged(float energy = 50f)
        {
            var weapon = Create();
            weapon.Attach(_carrier, _trigger);
            _loop.Tick(energy / _config.RefillPerSecond);
            return weapon;
        }

        private void Press(float deltaTime = 0.01f)
        {
            _trigger.IsFiring = true;
            _loop.Tick(deltaTime);
        }

        private ILaserBeamContacts BeamContacts()
        {
            Assert.That(_beams.TryGet(_beam.Entity, out var contacts), Is.True, "the beam is not registered");
            return contacts;
        }

        [Test]
        public void Attach_RegistersTheTriggerTheAimAndTheHits()
        {
            Create().Attach(_carrier, _trigger);

            Assert.That(_loop.LiveCount(SimulationPhase.Input), Is.EqualTo(1));
            Assert.That(_loop.LiveCount(SimulationPhase.Motion), Is.EqualTo(1));
            Assert.That(_loop.LiveCount(SimulationPhase.Logic), Is.EqualTo(1));
            Assert.That(_beams.TryGet(_beam.Entity, out _), Is.True);
        }

        [Test]
        public void Construct_TheTankStartsEmpty()
        {
            var weapon = Create();
            weapon.Attach(_carrier, _trigger);

            Press();

            Assert.That(_beam.IsShown, Is.False);
            Assert.That(weapon.Energy.Value, Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void Tick_PressWithEnergy_ShowsTheBeamAtTheMuzzleOnTheSameTick()
        {
            _carrier.Pose.Position = new Vector2(1f, 1f);
            _carrier.Pose.Angle = 90f; // Facing -X.
            var weapon = CreateCharged();

            Press();

            Assert.That(weapon.IsBeamOn, Is.True);
            Assert.That(_beam.IsShown, Is.True);
            var pose = _beam.Entity.Pose;
            Assert.That(pose.Position.x, Is.EqualTo(0.5f).Within(Tolerance));
            Assert.That(pose.Position.y, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(pose.Angle, Is.EqualTo(90f).Within(Tolerance));
        }

        [Test]
        public void Tick_BeamOn_FollowsTheCarrier()
        {
            CreateCharged();
            Press();

            _carrier.Pose.Position = new Vector2(3f, 0f);
            _carrier.Pose.Angle = 180f; // Facing -Y.
            _loop.Tick(0.01f);

            var pose = _beam.Entity.Pose;
            Assert.That(pose.Position.x, Is.EqualTo(3f).Within(Tolerance));
            Assert.That(pose.Position.y, Is.EqualTo(-0.5f).Within(Tolerance));
            Assert.That(pose.Angle, Is.EqualTo(180f).Within(Tolerance));
            Assert.That(_beam.ShowCount, Is.EqualTo(1), "the beam is shown once and then only moved");
        }

        [Test]
        public void Tick_Released_PutsTheBeamOutAtOnce()
        {
            var weapon = CreateCharged();
            Press();

            _trigger.IsFiring = false;
            _loop.Tick(0.01f);

            Assert.That(weapon.IsBeamOn, Is.False);
            Assert.That(_beam.IsShown, Is.False);
        }

        [Test]
        public void Tick_TankRunsDry_PutsTheBeamOut()
        {
            CreateCharged(25f);
            Press();

            _loop.Tick(1f);

            Assert.That(_beam.IsShown, Is.False);
        }

        [Test]
        public void Tick_ATargetTouchesTheBeam_IsHitOnTheSameTick()
        {
            var target = new TestEntity();
            var health = new RecordingDamageable();
            _damage.Add(target, health);
            CreateCharged();
            Press();

            // Collisions run between Motion and Logic.
            BeamContacts().Entered(target);
            _loop.Tick(0.01f);

            Assert.That(health.Hits, Is.EqualTo(new[] { 3 }));
        }

        [Test]
        public void Tick_BeamGoesOut_ForgetsItsTargets()
        {
            var target = new TestEntity();
            var health = new RecordingDamageable();
            _damage.Add(target, health);
            CreateCharged();
            Press();
            BeamContacts().Entered(target);
            _loop.Tick(0.01f);

            _trigger.IsFiring = false;
            _loop.Tick(0.01f);
            _loop.Tick(1f);

            Assert.That(health.Hits.Count, Is.EqualTo(1));
        }

        [Test]
        public void Energy_ReportsTheTank()
        {
            var weapon = CreateCharged(40f);

            Assert.That(weapon.Energy.Value, Is.EqualTo(0.4f).Within(Tolerance));
            Assert.That(weapon.SecondsToFull.Value, Is.EqualTo(6f).Within(Tolerance));
        }

        [Test]
        public void Dispose_WhileFiring_PutsTheBeamOutAndTakesEverythingOff()
        {
            var weapon = Create();
            var attachment = weapon.Attach(_carrier, _trigger);
            _loop.Tick(5f);
            Press();

            attachment.Dispose();

            Assert.That(_beam.IsShown, Is.False);
            Assert.That(weapon.IsBeamOn, Is.False);
            Assert.That(_beams.TryGet(_beam.Entity, out _), Is.False);
            Assert.That(_loop.LiveCount(SimulationPhase.Input), Is.EqualTo(0));
            Assert.That(_loop.LiveCount(SimulationPhase.Motion), Is.EqualTo(0));
            Assert.That(_loop.LiveCount(SimulationPhase.Logic), Is.EqualTo(0));
        }

        [Test]
        public void Dispose_PutsTheBeamOutBeforeTheBeamLeavesTheRegistry()
        {
            var weapon = Create();
            var attachment = weapon.Attach(_carrier, _trigger);
            _loop.Tick(5f);
            Press();
            var registeredWhileHiding = false;
            _beam.OnHide = () => registeredWhileHiding = _beams.TryGet(_beam.Entity, out _);

            attachment.Dispose();

            Assert.That(registeredWhileHiding, Is.True);
        }

        [Test]
        public void Attach_Again_KeepsTheEnergyAndWaitsForAFreshPress()
        {
            var weapon = Create();
            var attachment = weapon.Attach(_carrier, _trigger);
            _loop.Tick(5f);
            Press();
            attachment.Dispose();

            weapon.Attach(_carrier, _trigger);
            _loop.Tick(0.01f);
            Assert.That(_beam.IsShown, Is.False, "the trigger is still held");
            Assert.That(weapon.Energy.Value, Is.GreaterThan(0.4f));

            _trigger.IsFiring = false;
            _loop.Tick(0.01f);
            Press();
            Assert.That(_beam.IsShown, Is.True);
        }

        [Test]
        public void Tick_ShowingTheBeamFails_LeavesTheBeamOffAndPropagates()
        {
            var weapon = CreateCharged();
            _beam.FailOnShow = true;
            _trigger.IsFiring = true;

            Assert.Throws<InvalidOperationException>(() => _loop.Tick(0.01f));
            Assert.That(weapon.IsBeamOn, Is.False);
        }

        [Test]
        public void Attach_Twice_Throws()
        {
            var weapon = Create();
            weapon.Attach(_carrier, _trigger);

            Assert.Throws<InvalidOperationException>(() => weapon.Attach(_carrier, _trigger));
        }

        [Test]
        public void Attach_AfterDispose_Works()
        {
            var weapon = Create();
            weapon.Attach(_carrier, _trigger).Dispose();

            Assert.DoesNotThrow(() => weapon.Attach(_carrier, _trigger));
        }

        [Test]
        public void Attach_NullArguments_Throw()
        {
            var weapon = Create();

            Assert.Throws<ArgumentNullException>(() => weapon.Attach(null, _trigger));
            Assert.Throws<ArgumentNullException>(() => weapon.Attach(_carrier, null));
        }

        [Test]
        public void Construct_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new LaserWeapon(null, _loop, _beam, _beams, _damage));
            Assert.Throws<ArgumentNullException>(() => new LaserWeapon(_config, null, _beam, _beams, _damage));
            Assert.Throws<ArgumentNullException>(() => new LaserWeapon(_config, _loop, null, _beams, _damage));
            Assert.Throws<ArgumentNullException>(() => new LaserWeapon(_config, _loop, _beam, null, _damage));
            Assert.Throws<ArgumentNullException>(() => new LaserWeapon(_config, _loop, _beam, _beams, null));
        }
    }
}

using System;
using Asteroids.Weapons.Laser;
using NUnit.Framework;

namespace Asteroids.Weapons.Tests
{
    [TestFixture]
    public sealed class LaserEnergyTests
    {
        private const float Tolerance = 1e-4f;

        private TestLaserConfig _config;
        private LaserEnergy _energy;

        [SetUp]
        public void SetUp()
        {
            // Full at 100, drains 50/s, refills 10/s after 1 s, a press needs 20.
            _config = new TestLaserConfig
            {
                MaxEnergy = 100f, DrainPerSecond = 50f, RefillPerSecond = 10f, RefillDelay = 1f, MinEnergyToFire = 20f
            };
            _energy = new LaserEnergy(_config);
        }

        /// <summary>Refills from the current level with the trigger released, starting right away.</summary>
        private void RefillFor(float seconds) => _energy.Tick(seconds, false);

        [Test]
        public void New_StartsEmptyAndNeedsAFullRefill()
        {
            Assert.That(_energy.Current, Is.EqualTo(0f));
            Assert.That(_energy.IsFiring, Is.False);
            Assert.That(_energy.Energy.Value, Is.EqualTo(0f));
            Assert.That(_energy.SecondsToFull.Value, Is.EqualTo(10f).Within(Tolerance));
        }

        [Test]
        public void Tick_Released_RefillsAtTheConfiguredRate()
        {
            RefillFor(1f);

            Assert.That(_energy.Current, Is.EqualTo(10f).Within(Tolerance));
            Assert.That(_energy.Energy.Value, Is.EqualTo(0.1f).Within(Tolerance));
            Assert.That(_energy.SecondsToFull.Value, Is.EqualTo(9f).Within(Tolerance));
        }

        [Test]
        public void Tick_Refilling_StopsAtAFullTank()
        {
            RefillFor(20f);

            Assert.That(_energy.Current, Is.EqualTo(100f).Within(Tolerance));
            Assert.That(_energy.Energy.Value, Is.EqualTo(1f));
            Assert.That(_energy.SecondsToFull.Value, Is.EqualTo(0f));
        }

        [Test]
        public void Tick_PressBelowTheMinimum_DoesNotFire()
        {
            RefillFor(1.9f);

            Assert.That(_energy.Tick(0.01f, true), Is.False);
            Assert.That(_energy.IsFiring, Is.False);
        }

        [Test]
        public void Tick_PressWithTheMinimum_FiresAndDrainsOnTheSameTick()
        {
            RefillFor(3f);

            Assert.That(_energy.Tick(0.1f, true), Is.True);
            Assert.That(_energy.IsFiring, Is.True);
            Assert.That(_energy.Current, Is.EqualTo(25f).Within(Tolerance));
        }

        [Test]
        public void Tick_Held_KeepsFiringAndNeverRefills()
        {
            RefillFor(10f);
            _energy.Tick(0.1f, true);

            Assert.That(_energy.Tick(0.5f, true), Is.True);
            Assert.That(_energy.Tick(0.5f, true), Is.True);
            Assert.That(_energy.Current, Is.EqualTo(45f).Within(Tolerance));
        }

        [Test]
        public void Tick_Released_StopsAtOnce()
        {
            RefillFor(10f);
            _energy.Tick(0.1f, true);

            Assert.That(_energy.Tick(0.1f, false), Is.False);
            Assert.That(_energy.IsFiring, Is.False);
            Assert.That(_energy.Current, Is.EqualTo(95f).Within(Tolerance));
        }

        [Test]
        public void Tick_HeldFromTheStart_NeverStartsWithoutAPress()
        {
            for (var i = 0; i < 50; i++) Assert.That(_energy.Tick(0.1f, true), Is.False);
            Assert.That(_energy.Current, Is.EqualTo(50f).Within(Tolerance));

            _energy.Tick(0.1f, false);

            Assert.That(_energy.Tick(0.1f, true), Is.True);
        }

        [Test]
        public void Tick_TankRunsDry_StopsAndStaysOffWhileHeld()
        {
            RefillFor(3f);
            _energy.Tick(0.1f, true); // 25 left.

            Assert.That(_energy.Tick(1f, true), Is.False);
            Assert.That(_energy.Current, Is.EqualTo(0f));
            Assert.That(_energy.Tick(5f, true), Is.False);
        }

        [Test]
        public void Tick_AfterRunningDry_ARePressNeedsTheMinimumAgain()
        {
            RefillFor(3f);
            _energy.Tick(0.1f, true);
            _energy.Tick(1f, true); // Dry.
            _energy.Tick(2f, false); // 1 s of delay, then 10 energy.

            Assert.That(_energy.Tick(0.01f, true), Is.False);

            _energy.Tick(1f, false); // 20.
            Assert.That(_energy.Tick(0.01f, true), Is.True);
        }

        [Test]
        public void Tick_AfterTheBeamStops_WaitsTheRefillDelayFirst()
        {
            RefillFor(5f);
            _energy.Tick(0.2f, true); // 40 left.
            _energy.Tick(0.1f, false); // Stopped.

            _energy.Tick(0.5f, false);
            Assert.That(_energy.Current, Is.EqualTo(40f).Within(Tolerance));

            // Half of this tick finishes the wait, the other half refills.
            _energy.Tick(1f, false);
            Assert.That(_energy.Current, Is.EqualTo(45f).Within(Tolerance));
        }

        [Test]
        public void SecondsToFull_IncludesTheRemainingDelay()
        {
            RefillFor(5f);
            _energy.Tick(0.2f, true); // 40 left, so 6 s of refill.

            Assert.That(_energy.SecondsToFull.Value, Is.EqualTo(7f).Within(Tolerance), "while firing");

            _energy.Tick(0.1f, false);
            _energy.Tick(0.25f, false);
            Assert.That(_energy.SecondsToFull.Value, Is.EqualTo(6.75f).Within(Tolerance), "while waiting");
        }

        [Test]
        public void Interrupt_StopsTheBeamAndWaitsForAFreshPress()
        {
            RefillFor(10f);
            _energy.Tick(0.1f, true);

            _energy.Interrupt();

            Assert.That(_energy.IsFiring, Is.False);
            Assert.That(_energy.Tick(0.1f, true), Is.False, "still held");
            _energy.Tick(0.1f, false);
            Assert.That(_energy.Tick(0.1f, true), Is.True, "pressed again");
        }

        [Test]
        public void Interrupt_StartsTheRefillDelay()
        {
            RefillFor(10f);
            _energy.Tick(0.1f, true); // 95 left.

            _energy.Interrupt();
            _energy.Tick(0.5f, false);

            Assert.That(_energy.Current, Is.EqualTo(95f).Within(Tolerance));
        }

        [Test]
        public void Interrupt_WhileOff_DoesNothing()
        {
            RefillFor(3f);

            _energy.Interrupt();

            Assert.That(_energy.Tick(0.01f, true), Is.True);
        }

        [Test]
        public void Construct_NullConfig_Throws() =>
            Assert.Throws<ArgumentNullException>(() => new LaserEnergy(null));

        [Test]
        public void Reset_EmptiesTheTankAndStopsTheBeam()
        {
            RefillFor(5f);
            _energy.Tick(0.1f, true);
            Assert.That(_energy.IsFiring, Is.True);

            _energy.Reset();

            Assert.That(_energy.Current, Is.EqualTo(0f));
            Assert.That(_energy.IsFiring, Is.False);
            Assert.That(_energy.Energy.Value, Is.EqualTo(0f));
            Assert.That(_energy.SecondsToFull.Value, Is.EqualTo(10f).Within(Tolerance));
        }

        [Test]
        public void Reset_RefillStartsRightAway()
        {
            RefillFor(5f);
            _energy.Tick(0.1f, true);
            _energy.Tick(0.1f, false);

            _energy.Reset();
            RefillFor(1f);

            Assert.That(_energy.Current, Is.EqualTo(10f).Within(Tolerance));
        }
    }
}

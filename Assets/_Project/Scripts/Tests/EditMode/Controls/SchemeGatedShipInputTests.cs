using System;
using Asteroids.Core;
using NUnit.Framework;

namespace Asteroids.Controls.Tests
{
    /// <summary>
    /// The ship input the game actually reads. Two rules: everything is idle outside <see cref="InputScheme.Gameplay"/>,
    /// and after entering Gameplay a fire button must be seen released once (by <see cref="SchemeGatedShipInput.Tick"/>)
    /// before it fires. The second rule is what stops Space (Submit on the end screen) from firing the moment the run
    /// restarts.
    /// </summary>
    [TestFixture]
    public sealed class SchemeGatedShipInputTests
    {
        private TestAxes _axes;
        private InputSchemes _schemes;
        private SchemeGatedShipInput _input;

        private IFireInput Fire => _input.For(WeaponSlot.Primary);
        private IFireInput AltFire => _input.For(WeaponSlot.Alternate);

        [SetUp]
        public void SetUp()
        {
            _axes = new TestAxes();
            _schemes = new InputSchemes();
            _input = new SchemeGatedShipInput(new ShipInput(_axes), _schemes);
        }

        [TearDown]
        public void TearDown() => _input.Dispose();

        /// <summary>Enters Gameplay with nothing held and lets one frame pass, the normal start of a run.</summary>
        private void PlayWithNothingHeld()
        {
            _schemes.Activate(InputScheme.Gameplay);
            _input.Tick();
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new SchemeGatedShipInput(null, _schemes));
            Assert.Throws<ArgumentNullException>(() => _ = new SchemeGatedShipInput(new ShipInput(_axes), null));
        }

        [Test]
        public void Movement_InGameplay_PassesThrough()
        {
            PlayWithNothingHeld();
            _axes[ShipInputAxes.Turn] = -0.5f;
            _axes[ShipInputAxes.Thrust] = 1f;

            Assert.That(_input.Turn, Is.EqualTo(-0.5f));
            Assert.That(_input.Thrust, Is.EqualTo(1f));
        }

        [TestCase(InputScheme.None)]
        [TestCase(InputScheme.UI)]
        public void Everything_OutsideGameplay_IsIdle(InputScheme scheme)
        {
            PlayWithNothingHeld();
            _schemes.Activate(scheme);
            _axes[ShipInputAxes.Turn] = 1f;
            _axes[ShipInputAxes.Thrust] = 1f;
            _axes[ShipInputAxes.Fire] = 1f;
            _axes[ShipInputAxes.AltFire] = 1f;
            _input.Tick();

            Assert.That(_input.Turn, Is.Zero);
            Assert.That(_input.Thrust, Is.Zero);
            Assert.That(Fire.IsFiring, Is.False);
            Assert.That(AltFire.IsFiring, Is.False);
        }

        [Test]
        public void Fire_PressedAfterARelease_Fires()
        {
            PlayWithNothingHeld();

            _axes[ShipInputAxes.Fire] = 1f;

            Assert.That(Fire.IsFiring, Is.True);
        }

        /// <summary>The Replay case: Space held as Submit while the run restarts.</summary>
        [Test]
        public void Fire_HeldWhileEnteringGameplay_StaysSilentUntilReleased()
        {
            _axes[ShipInputAxes.Fire] = 1f;
            _schemes.Activate(InputScheme.Gameplay);
            _input.Tick();
            _input.Tick();
            Assert.That(Fire.IsFiring, Is.False);

            _axes[ShipInputAxes.Fire] = 0f;
            _input.Tick();
            _axes[ShipInputAxes.Fire] = 1f;

            Assert.That(Fire.IsFiring, Is.True);
        }

        /// <summary>Weapons may read before the first frame tick; a never-seen-released button must not fire then.</summary>
        [Test]
        public void Fire_BeforeAnyTickInGameplay_IsSilent()
        {
            _schemes.Activate(InputScheme.Gameplay);
            _axes[ShipInputAxes.Fire] = 1f;

            Assert.That(Fire.IsFiring, Is.False);
        }

        [Test]
        public void ReenteringGameplay_AsksForAFreshRelease()
        {
            PlayWithNothingHeld();
            _schemes.Activate(InputScheme.UI);
            _axes[ShipInputAxes.Fire] = 1f;

            _schemes.Activate(InputScheme.Gameplay);
            _input.Tick();

            Assert.That(Fire.IsFiring, Is.False);
        }

        /// <summary>Releases are sampled by Tick, not when a weapon happens to read, so none is missed.</summary>
        [Test]
        public void Release_SeenOnlyByTick_StillArms()
        {
            _axes[ShipInputAxes.Fire] = 1f;
            _schemes.Activate(InputScheme.Gameplay);

            _axes[ShipInputAxes.Fire] = 0f;
            _input.Tick();
            _axes[ShipInputAxes.Fire] = 1f;

            Assert.That(Fire.IsFiring, Is.True);
        }

        [Test]
        public void Triggers_AreGatedIndependently()
        {
            _axes[ShipInputAxes.Fire] = 1f;
            _schemes.Activate(InputScheme.Gameplay);
            _input.Tick();
            _axes[ShipInputAxes.AltFire] = 1f;

            Assert.That(Fire.IsFiring, Is.False);
            Assert.That(AltFire.IsFiring, Is.True);
        }

        /// <summary>Holding thrust through a restart is harmless, so movement is not release-gated.</summary>
        [Test]
        public void Movement_HeldWhileEnteringGameplay_AppliesAtOnce()
        {
            _axes[ShipInputAxes.Thrust] = 1f;

            _schemes.Activate(InputScheme.Gameplay);

            Assert.That(_input.Thrust, Is.EqualTo(1f));
        }

        [Test]
        public void For_ReturnsTheSameTriggerEveryTime() =>
            Assert.That(_input.For(WeaponSlot.Primary), Is.SameAs(_input.For(WeaponSlot.Primary)));

        [Test]
        public void For_UnknownSlot_Throws() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => _input.For((WeaponSlot)42));

        [Test]
        public void Dispose_StopsDisarmingOnSchemeChanges()
        {
            PlayWithNothingHeld();
            _input.Dispose();
            _schemes.Activate(InputScheme.UI);
            _schemes.Activate(InputScheme.Gameplay);
            _axes[ShipInputAxes.Fire] = 1f;

            Assert.That(Fire.IsFiring, Is.True);
        }
    }
}

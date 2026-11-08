using System;
using Asteroids.Core;
using NUnit.Framework;

namespace Asteroids.Controls.Tests
{
    /// <summary>
    /// One trigger on its own. It starts disarmed (as if Gameplay had just begun), arms when <see cref="GameplayFireTrigger.Sample"/>
    /// sees the button up in Gameplay, and fires only while armed, in Gameplay and held.
    /// <see cref="SchemeGatedShipInputTests"/> covers when the owner disarms and samples.
    /// </summary>
    [TestFixture]
    public sealed class GameplayFireTriggerTests
    {
        private sealed class FakeButton : IFireInput
        {
            public bool IsFiring { get; set; }
        }

        private FakeButton _button;
        private InputSchemes _schemes;
        private GameplayFireTrigger _trigger;

        [SetUp]
        public void SetUp()
        {
            _button = new FakeButton();
            _schemes = new InputSchemes();
            _schemes.Activate(InputScheme.Gameplay);
            _trigger = new GameplayFireTrigger(_button, _schemes);
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new GameplayFireTrigger(null, _schemes));
            Assert.Throws<ArgumentNullException>(() => _ = new GameplayFireTrigger(_button, null));
        }

        [Test]
        public void New_IsDisarmed()
        {
            _button.IsFiring = true;

            Assert.That(_trigger.IsFiring, Is.False);
        }

        [Test]
        public void Sample_ButtonUp_Arms()
        {
            _trigger.Sample();
            _button.IsFiring = true;

            Assert.That(_trigger.IsFiring, Is.True);
        }

        [Test]
        public void Sample_ButtonHeld_StaysDisarmed()
        {
            _button.IsFiring = true;

            _trigger.Sample();

            Assert.That(_trigger.IsFiring, Is.False);
        }

        /// <summary>A release outside Gameplay does not count; it has to happen after Gameplay began.</summary>
        [Test]
        public void Sample_OutsideGameplay_DoesNotArm()
        {
            _schemes.Activate(InputScheme.UI);
            _trigger.Sample();
            _schemes.Activate(InputScheme.Gameplay);
            _button.IsFiring = true;

            Assert.That(_trigger.IsFiring, Is.False);
        }

        [Test]
        public void IsFiring_Armed_OutsideGameplay_IsFalse()
        {
            _trigger.Sample();
            _button.IsFiring = true;

            _schemes.Activate(InputScheme.None);

            Assert.That(_trigger.IsFiring, Is.False);
        }

        [Test]
        public void Disarm_RequiresAnotherRelease()
        {
            _trigger.Sample();
            _button.IsFiring = true;

            _trigger.Disarm();
            _trigger.Sample();

            Assert.That(_trigger.IsFiring, Is.False);
        }

        /// <summary>Once armed, holding and releasing repeatedly keeps firing normally.</summary>
        [Test]
        public void Armed_StaysArmedAcrossPresses()
        {
            _trigger.Sample();

            _button.IsFiring = true;
            _trigger.Sample();
            Assert.That(_trigger.IsFiring, Is.True);

            _button.IsFiring = false;
            _trigger.Sample();
            _button.IsFiring = true;
            Assert.That(_trigger.IsFiring, Is.True);
        }
    }
}

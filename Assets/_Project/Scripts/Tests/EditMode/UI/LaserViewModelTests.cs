using System;
using NUnit.Framework;

namespace Asteroids.UI.Tests
{
    /// <summary>
    /// Energy is shown in whole percent rounded down, the refill time in tenths rounded up, and the full text when
    /// nothing is left to refill. The wording comes from <see cref="TestLaserHudConfig"/> ("{0}", "{0:0.0}", "full"),
    /// never from the shipped <c>HudConfig</c>.
    /// </summary>
    [TestFixture]
    public sealed class LaserViewModelTests
    {
        private static LaserViewModel Create(FakeLaserStatus laser) => new(laser, new TestLaserHudConfig());

        [Test]
        public void EnergyText_ShowsThePercentRoundedDown()
        {
            using var viewModel = Create(new FakeLaserStatus(0.739f, 0f));

            Assert.That(viewModel.EnergyText.Value, Is.EqualTo("73"));
        }

        [Test]
        public void EnergyText_AlmostFull_IsNotShownAsFull()
        {
            using var viewModel = Create(new FakeLaserStatus(0.999f, 0.1f));

            Assert.That(viewModel.EnergyText.Value, Is.EqualTo("99"));
        }

        [Test]
        public void EnergyText_FollowsTheLaser()
        {
            var laser = new FakeLaserStatus(0f, 10f);
            using var viewModel = Create(laser);

            laser.EnergyValue.Value = 1f;

            Assert.That(viewModel.EnergyText.Value, Is.EqualTo("100"));
        }

        [Test]
        public void RefillText_ShowsTheSecondsRoundedUpToATenth()
        {
            using var viewModel = Create(new FakeLaserStatus(0.5f, 4.21f));

            Assert.That(viewModel.RefillText.Value, Is.EqualTo("4.3"));
        }

        /// <summary>The config picks the wording, not the precision: the seconds reach the format already rounded up.</summary>
        [Test]
        public void RefillText_FinerFormat_StillShowsTenths()
        {
            var config = new TestLaserHudConfig { RefillFormat = "{0:0.00}" };

            using var viewModel = new LaserViewModel(new FakeLaserStatus(0.5f, 4.21f), config);

            Assert.That(viewModel.RefillText.Value, Is.EqualTo("4.30"));
        }

        [Test]
        public void RefillText_ATinyRest_NeverReadsZero()
        {
            using var viewModel = Create(new FakeLaserStatus(0.99f, 0.001f));

            Assert.That(viewModel.RefillText.Value, Is.EqualTo("0.1"));
        }

        [Test]
        public void RefillText_FullTank_ShowsTheFullText()
        {
            var laser = new FakeLaserStatus(0.9f, 2f);
            using var viewModel = Create(laser);

            laser.SecondsValue.Value = 0f;

            Assert.That(viewModel.RefillText.Value, Is.EqualTo("full"));
        }

        [Test]
        public void Texts_UseTheConfiguredFormats()
        {
            var config = new TestLaserHudConfig { EnergyFormat = "E{0}", RefillFormat = "R{0:0.0}" };

            using var viewModel = new LaserViewModel(new FakeLaserStatus(0.5f, 2f), config);

            Assert.That(viewModel.EnergyText.Value, Is.EqualTo("E50"));
            Assert.That(viewModel.RefillText.Value, Is.EqualTo("R2.0"));
        }

        [Test]
        [SetCulture("de-DE")]
        public void RefillText_UnderACommaCulture_UsesADot()
        {
            using var viewModel = Create(new FakeLaserStatus(0.5f, 1.5f));

            Assert.That(viewModel.RefillText.Value, Is.EqualTo("1.5"));
        }

        [Test]
        public void Dispose_StopsFollowingTheLaser()
        {
            var laser = new FakeLaserStatus(0.2f, 5f);
            var viewModel = Create(laser);

            viewModel.Dispose();
            laser.EnergyValue.Value = 0.8f;
            laser.SecondsValue.Value = 0f;

            Assert.That(viewModel.EnergyText.Value, Is.EqualTo("20"));
            Assert.That(viewModel.RefillText.Value, Is.EqualTo("5.0"));
        }

        [Test]
        public void Construct_NullLaser_Throws() =>
            Assert.Throws<ArgumentNullException>(() => new LaserViewModel(null, new TestLaserHudConfig()));

        [Test]
        public void Construct_NullConfig_Throws() =>
            Assert.Throws<ArgumentNullException>(() => new LaserViewModel(new FakeLaserStatus(1f, 0f), null));

        /// <summary>A broken format fails at startup with the validator's message, not on the first shot.</summary>
        [Test]
        public void Construct_InvalidConfig_Throws()
        {
            var config = new TestLaserHudConfig { RefillFormat = "{1}" };

            var error = Assert.Throws<InvalidOperationException>(() =>
                new LaserViewModel(new FakeLaserStatus(1f, 0f), config));
            StringAssert.Contains("Refill format", error.Message);
        }
    }
}

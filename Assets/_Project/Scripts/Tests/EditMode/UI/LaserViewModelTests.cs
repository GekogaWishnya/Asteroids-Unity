using System;
using NUnit.Framework;

namespace Asteroids.UI.Tests
{
    [TestFixture]
    public sealed class LaserViewModelTests
    {
        [Test]
        public void EnergyText_ShowsThePercentRoundedDown()
        {
            var laser = new FakeLaserStatus(0.739f, 0f);

            using var viewModel = new LaserViewModel(laser);

            Assert.That(viewModel.EnergyText.Value, Is.EqualTo("LASER 73%"));
        }

        [Test]
        public void EnergyText_AlmostFull_IsNotShownAsFull()
        {
            using var viewModel = new LaserViewModel(new FakeLaserStatus(0.999f, 0.1f));

            Assert.That(viewModel.EnergyText.Value, Is.EqualTo("LASER 99%"));
        }

        [Test]
        public void EnergyText_FollowsTheLaser()
        {
            var laser = new FakeLaserStatus(0f, 10f);
            using var viewModel = new LaserViewModel(laser);

            laser.EnergyValue.Value = 1f;

            Assert.That(viewModel.EnergyText.Value, Is.EqualTo("LASER 100%"));
        }

        [Test]
        public void RefillText_ShowsTheSecondsRoundedUpToATenth()
        {
            using var viewModel = new LaserViewModel(new FakeLaserStatus(0.5f, 4.21f));

            Assert.That(viewModel.RefillText.Value, Is.EqualTo("FULL IN 4.3s"));
        }

        [Test]
        public void RefillText_ATinyRest_NeverReadsZero()
        {
            using var viewModel = new LaserViewModel(new FakeLaserStatus(0.99f, 0.001f));

            Assert.That(viewModel.RefillText.Value, Is.EqualTo("FULL IN 0.1s"));
        }

        [Test]
        public void RefillText_FullTank_ReadsFull()
        {
            var laser = new FakeLaserStatus(0.9f, 2f);
            using var viewModel = new LaserViewModel(laser);

            laser.SecondsValue.Value = 0f;

            Assert.That(viewModel.RefillText.Value, Is.EqualTo("FULL"));
        }

        [Test]
        public void Dispose_StopsFollowingTheLaser()
        {
            var laser = new FakeLaserStatus(0.2f, 5f);
            var viewModel = new LaserViewModel(laser);

            viewModel.Dispose();
            laser.EnergyValue.Value = 0.8f;
            laser.SecondsValue.Value = 0f;

            Assert.That(viewModel.EnergyText.Value, Is.EqualTo("LASER 20%"));
            Assert.That(viewModel.RefillText.Value, Is.EqualTo("FULL IN 5.0s"));
        }

        [Test]
        public void Construct_Null_Throws() =>
            Assert.Throws<ArgumentNullException>(() => new LaserViewModel(null));
    }
}

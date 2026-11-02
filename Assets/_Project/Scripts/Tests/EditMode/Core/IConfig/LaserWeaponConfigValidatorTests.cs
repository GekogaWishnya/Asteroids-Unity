using System;
using NUnit.Framework;

namespace Asteroids.Core.IConfigs.Tests
{
    [TestFixture]
    public sealed class LaserWeaponConfigValidatorTests
    {
        private sealed class Config : ILaserWeaponConfig
        {
            public float MaxEnergy { get; set; } = 100f;
            public float DrainPerSecond { get; set; } = 50f;
            public float RefillPerSecond { get; set; } = 5f;
            public float RefillDelay { get; set; } = 1f;
            public float MinEnergyToFire { get; set; } = 20f;
            public int HitDamage { get; set; } = 1;
            public float HitInterval { get; set; } = 0.1f;
            public float MuzzleOffset { get; set; } = 0.5f;
        }

        [Test]
        public void Validate_ValidConfig_ReportsNothing() =>
            Assert.That(LaserWeaponConfigValidator.Validate(new Config()), Is.Empty);

        [Test]
        public void Validate_ZeroRefillDelayAndMuzzleOffset_ReportsNothing() =>
            Assert.That(
                LaserWeaponConfigValidator.Validate(new Config { RefillDelay = 0f, MuzzleOffset = 0f }), Is.Empty);

        [Test]
        public void Validate_MinEnergyEqualToMax_ReportsNothing() =>
            Assert.That(
                LaserWeaponConfigValidator.Validate(new Config { MinEnergyToFire = 100f, MaxEnergy = 100f }), Is.Empty);

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Validate_MaxEnergyNotPositive_ReportsIt(float value) =>
            AssertSingleProblem(new Config { MaxEnergy = value, MinEnergyToFire = 1f }, "Max energy");

        [TestCase(0f)]
        [TestCase(-1f)]
        public void Validate_DrainNotPositive_ReportsIt(float value) =>
            AssertSingleProblem(new Config { DrainPerSecond = value }, "Drain per second");

        [TestCase(0f)]
        [TestCase(float.NaN)]
        public void Validate_RefillNotPositive_ReportsIt(float value) =>
            AssertSingleProblem(new Config { RefillPerSecond = value }, "Refill per second");

        [TestCase(-0.1f)]
        [TestCase(float.NaN)]
        public void Validate_RefillDelayNegative_ReportsIt(float value) =>
            AssertSingleProblem(new Config { RefillDelay = value }, "Refill delay");

        [TestCase(0f)]
        [TestCase(-1f)]
        public void Validate_MinEnergyNotPositive_ReportsIt(float value) =>
            AssertSingleProblem(new Config { MinEnergyToFire = value }, "Min energy to fire");

        [Test]
        public void Validate_MinEnergyAboveMax_ReportsIt() =>
            AssertSingleProblem(new Config { MinEnergyToFire = 101f, MaxEnergy = 100f }, "Min energy to fire");

        [Test]
        public void Validate_HitDamageBelowOne_ReportsIt() =>
            AssertSingleProblem(new Config { HitDamage = 0 }, "Hit damage");

        [TestCase(0f)]
        [TestCase(float.PositiveInfinity)]
        public void Validate_HitIntervalNotPositive_ReportsIt(float value) =>
            AssertSingleProblem(new Config { HitInterval = value }, "Hit interval");

        [Test]
        public void Validate_NegativeMuzzleOffset_ReportsIt() =>
            AssertSingleProblem(new Config { MuzzleOffset = -0.5f }, "Muzzle offset");

        [Test]
        public void Validate_SeveralProblems_ReportsEveryOne()
        {
            var config = new Config { DrainPerSecond = 0f, HitDamage = 0, HitInterval = 0f };

            Assert.That(LaserWeaponConfigValidator.Validate(config).Count, Is.EqualTo(3));
        }

        [Test]
        public void Validate_Null_Throws() =>
            Assert.Throws<ArgumentNullException>(() => LaserWeaponConfigValidator.Validate(null));

        private static void AssertSingleProblem(ILaserWeaponConfig config, string subject)
        {
            var problems = LaserWeaponConfigValidator.Validate(config);

            Assert.That(problems.Count, Is.EqualTo(1), string.Join("; ", problems));
            Assert.That(problems[0], Does.StartWith(subject));
        }
    }
}

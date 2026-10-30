using System;
using NUnit.Framework;

namespace Asteroids.Core.IConfigs.Tests
{
    [TestFixture]
    public sealed class ProjectileWeaponConfigValidatorTests
    {
        private sealed class Config : IProjectileWeaponConfig
        {
            public float FireInterval { get; set; } = 0.25f;
            public float MuzzleOffset { get; set; } = 0.5f;
            public float ProjectileSpeed { get; set; } = 12f;
            public float ProjectileLifetime { get; set; } = 1f;
            public int ProjectileDamage { get; set; } = 1;
            public int PoolPrewarm { get; set; } = 8;
            public int PoolMaxIdle { get; set; } = 16;
        }

        [Test]
        public void Validate_ValidConfig_ReportsNothing() =>
            Assert.That(ProjectileWeaponConfigValidator.Validate(new Config()), Is.Empty);

        [Test]
        public void Validate_ZeroMuzzleOffset_ReportsNothing() =>
            Assert.That(ProjectileWeaponConfigValidator.Validate(new Config { MuzzleOffset = 0f }), Is.Empty);

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Validate_FireIntervalNotPositive_ReportsIt(float value) =>
            AssertSingleProblem(new Config { FireInterval = value }, "Fire interval");

        [TestCase(-0.1f)]
        [TestCase(float.NaN)]
        public void Validate_MuzzleOffsetNegative_ReportsIt(float value) =>
            AssertSingleProblem(new Config { MuzzleOffset = value }, "Muzzle offset");

        [TestCase(0f)]
        [TestCase(-5f)]
        public void Validate_SpeedNotPositive_ReportsIt(float value) =>
            AssertSingleProblem(new Config { ProjectileSpeed = value }, "Projectile speed");

        [TestCase(0f)]
        [TestCase(-1f)]
        public void Validate_LifetimeNotPositive_ReportsIt(float value) =>
            AssertSingleProblem(new Config { ProjectileLifetime = value }, "Projectile lifetime");

        [Test]
        public void Validate_DamageBelowOne_ReportsIt() =>
            AssertSingleProblem(new Config { ProjectileDamage = 0 }, "Projectile damage");

        [Test]
        public void Validate_NegativePoolPrewarm_ReportsIt() =>
            AssertSingleProblem(new Config { PoolPrewarm = -1 }, "Pool prewarm");

        [Test]
        public void Validate_NegativePoolMaxIdle_ReportsIt() =>
            AssertSingleProblem(new Config { PoolMaxIdle = -1 }, "Pool max idle");

        [Test]
        public void Validate_SeveralProblems_ReportsEveryOne()
        {
            var config = new Config { FireInterval = 0f, ProjectileSpeed = 0f, ProjectileDamage = 0 };

            Assert.That(ProjectileWeaponConfigValidator.Validate(config).Count, Is.EqualTo(3));
        }

        [Test]
        public void Validate_Null_Throws() =>
            Assert.Throws<ArgumentNullException>(() => ProjectileWeaponConfigValidator.Validate(null));

        private static void AssertSingleProblem(IProjectileWeaponConfig config, string subject)
        {
            var problems = ProjectileWeaponConfigValidator.Validate(config);

            Assert.That(problems.Count, Is.EqualTo(1));
            Assert.That(problems[0], Does.StartWith(subject));
        }
    }
}

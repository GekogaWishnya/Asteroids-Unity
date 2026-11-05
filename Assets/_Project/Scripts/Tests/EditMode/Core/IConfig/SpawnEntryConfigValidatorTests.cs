using System;
using NUnit.Framework;

namespace Asteroids.Core.IConfigs.Tests
{
    [TestFixture]
    public sealed class SpawnEntryConfigValidatorTests
    {
        private sealed class Config : ISpawnEntryConfig
        {
            public float SpawnMargin { get; set; } = 1f;
            public float MaxEnteringSeconds { get; set; } = 5f;
        }

        [Test]
        public void Validate_NullConfig_Throws() =>
            Assert.Throws<ArgumentNullException>(() => SpawnEntryConfigValidator.Validate(null));

        [Test]
        public void Validate_ValidConfig_ReportsNothing()
        {
            Assert.That(SpawnEntryConfigValidator.Validate(new Config()), Is.Empty);
            Assert.That(SpawnEntryConfigValidator.Validate(new Config { SpawnMargin = 0f }), Is.Empty);
        }

        [TestCase(-0.1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Validate_BadMargin_Reports(float margin)
        {
            var problems = SpawnEntryConfigValidator.Validate(new Config { SpawnMargin = margin });

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("Spawn margin", problems[0]);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Validate_BadMaxEnteringSeconds_Reports(float seconds)
        {
            var problems = SpawnEntryConfigValidator.Validate(new Config { MaxEnteringSeconds = seconds });

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("Max entering seconds", problems[0]);
        }

        [Test]
        public void Validate_EveryProblem_IsReported() =>
            Assert.That(
                SpawnEntryConfigValidator.Validate(new Config { SpawnMargin = -1f, MaxEnteringSeconds = 0f }),
                Has.Count.EqualTo(2));
    }
}

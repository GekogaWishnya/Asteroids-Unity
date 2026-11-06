using System;
using NUnit.Framework;

namespace Asteroids.Core.IConfigs.Tests
{
    /// <summary>
    /// The score, health and laser HUD validators. Each one checks that its formats can be applied to the values
    /// the HUD passes them (ints for score, health and energy percent, a double for the refill seconds).
    /// They never judge the wording. The telemetry validator has its own fixture.
    /// </summary>
    [TestFixture]
    public sealed class HudTextConfigValidatorTests
    {
        [Test]
        public void Validate_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => ScoreHudConfigValidator.Validate(null));
            Assert.Throws<ArgumentNullException>(() => HealthHudConfigValidator.Validate(null));
            Assert.Throws<ArgumentNullException>(() => LaserHudConfigValidator.Validate(null));
        }

        [Test]
        public void Validate_ValidConfigs_ReportNothing()
        {
            Assert.That(ScoreHudConfigValidator.Validate(new TestScoreHudConfig()), Is.Empty);
            Assert.That(HealthHudConfigValidator.Validate(new TestHealthHudConfig()), Is.Empty);
            Assert.That(LaserHudConfigValidator.Validate(new TestLaserHudConfig()), Is.Empty);
        }

        /// <summary>The shipped defaults in <c>HudConfig</c>, copied here only to prove that this shape passes.</summary>
        [Test]
        public void Validate_ShippedStyleFormats_ReportNothing()
        {
            Assert.That(ScoreHudConfigValidator.Validate(new TestScoreHudConfig { ScoreFormat = "SCORE {0:D6}" }), Is.Empty);
            Assert.That(HealthHudConfigValidator.Validate(new TestHealthHudConfig { HealthFormat = "HP {0}/{1}" }), Is.Empty);
            Assert.That(LaserHudConfigValidator.Validate(new TestLaserHudConfig
            {
                EnergyFormat = "LASER {0}%",
                RefillFormat = "FULL IN {0:0.0}s",
                FullText = "FULL"
            }), Is.Empty);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("{1}")]
        [TestCase("{0:D6")]
        public void Validate_BadScoreFormat_Reports(string format)
        {
            var problems = ScoreHudConfigValidator.Validate(new TestScoreHudConfig { ScoreFormat = format });

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("Score format", problems[0]);
        }

        /// <summary>"D" only works on integers. The score is an int, so it must pass.</summary>
        [Test]
        public void Validate_ScoreFormatWithIntegerPadding_ReportsNothing() =>
            Assert.That(ScoreHudConfigValidator.Validate(new TestScoreHudConfig { ScoreFormat = "{0:D8}" }), Is.Empty);

        [TestCase(null)]
        [TestCase("")]
        [TestCase("{0}/{1}/{2}")]
        public void Validate_BadHealthFormat_Reports(string format)
        {
            var problems = HealthHudConfigValidator.Validate(new TestHealthHudConfig { HealthFormat = format });

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("Health format", problems[0]);
        }

        [Test]
        public void Validate_HealthFormatShowingOnlyTheCurrent_ReportsNothing() =>
            Assert.That(HealthHudConfigValidator.Validate(new TestHealthHudConfig { HealthFormat = "HP {0}" }), Is.Empty);

        [Test]
        public void Validate_BadLaserFormats_ReportEach()
        {
            var config = new TestLaserHudConfig { EnergyFormat = "{1}", RefillFormat = "", FullText = null };

            var problems = LaserHudConfigValidator.Validate(config);

            Assert.That(problems, Has.Count.EqualTo(3));
            StringAssert.Contains("Energy format", problems[0]);
            StringAssert.Contains("Refill format", problems[1]);
            StringAssert.Contains("Full text", problems[2]);
        }

        /// <summary>An empty full text is allowed: the HUD may show nothing once the laser is full.</summary>
        [Test]
        public void Validate_EmptyFullText_ReportsNothing() =>
            Assert.That(LaserHudConfigValidator.Validate(new TestLaserHudConfig { FullText = "" }), Is.Empty);
    }
}

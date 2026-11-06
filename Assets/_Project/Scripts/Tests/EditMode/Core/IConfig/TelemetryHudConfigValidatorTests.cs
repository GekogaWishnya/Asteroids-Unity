using System;
using NUnit.Framework;

namespace Asteroids.Core.IConfigs.Tests
{
    /// <summary>
    /// The validator checks that each format can be applied to the values the HUD passes it
    /// (two for the position, one for the angle and one for the speed). It does not judge the wording.
    /// </summary>
    [TestFixture]
    public sealed class TelemetryHudConfigValidatorTests
    {
        [Test]
        public void Validate_Null_Throws() =>
            Assert.Throws<ArgumentNullException>(() => TelemetryHudConfigValidator.Validate(null));

        [Test]
        public void Validate_ValidConfig_ReportsNothing() =>
            Assert.That(TelemetryHudConfigValidator.Validate(new TestTelemetryHudConfig()), Is.Empty);

        /// <summary>The shipped defaults in <c>HudConfig</c>, copied here only to prove that this shape passes.</summary>
        [Test]
        public void Validate_ShippedStyleFormats_ReportsNothing()
        {
            var config = new TestTelemetryHudConfig
            {
                PositionFormat = "X {0:0.0}  Y {1:0.0}",
                AngleFormat = "ANG {0}°",
                SpeedFormat = "SPD {0:0.0}",
                NoShipPositionText = "X --  Y --",
                NoShipAngleText = "ANG --",
                NoShipSpeedText = "SPD --"
            };

            Assert.That(TelemetryHudConfigValidator.Validate(config), Is.Empty);
        }

        [Test]
        public void Validate_FormatsWithoutPlaceholders_ReportsNothing()
        {
            var config = new TestTelemetryHudConfig { PositionFormat = "X", AngleFormat = "A", SpeedFormat = "S" };

            Assert.That(TelemetryHudConfigValidator.Validate(config), Is.Empty);
        }

        [TestCase(null)]
        [TestCase("")]
        public void Validate_EmptyPositionFormat_Reports(string format)
        {
            var problems = TelemetryHudConfigValidator.Validate(new TestTelemetryHudConfig { PositionFormat = format });

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("Position format", problems[0]);
        }

        [TestCase("{0} {1} {2}")]
        [TestCase("{0:0.0")]
        [TestCase("X {a}")]
        public void Validate_BrokenPositionFormat_Reports(string format)
        {
            var problems = TelemetryHudConfigValidator.Validate(new TestTelemetryHudConfig { PositionFormat = format });

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("Position format", problems[0]);
        }

        [Test]
        public void Validate_AngleFormatAskingForASecondValue_Reports()
        {
            var problems = TelemetryHudConfigValidator.Validate(new TestTelemetryHudConfig { AngleFormat = "{0} {1}" });

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("Angle format", problems[0]);
        }

        [Test]
        public void Validate_SpeedFormatAskingForASecondValue_Reports()
        {
            var problems = TelemetryHudConfigValidator.Validate(new TestTelemetryHudConfig { SpeedFormat = "{1}" });

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("Speed format", problems[0]);
        }

        /// <summary>An empty no-ship text is allowed: the HUD may show nothing without a ship. Only null is reported.</summary>
        [Test]
        public void Validate_NoShipTexts_EmptyIsFineNullIsReported()
        {
            var empty = new TestTelemetryHudConfig { NoShipPositionText = "", NoShipAngleText = "", NoShipSpeedText = "" };
            var missing = new TestTelemetryHudConfig { NoShipPositionText = null, NoShipAngleText = null, NoShipSpeedText = null };

            Assert.That(TelemetryHudConfigValidator.Validate(empty), Is.Empty);
            Assert.That(TelemetryHudConfigValidator.Validate(missing), Has.Count.EqualTo(3));
        }

        [Test]
        public void Validate_SeveralProblems_ReportsEach()
        {
            var config = new TestTelemetryHudConfig { PositionFormat = "", AngleFormat = "{3}", NoShipSpeedText = null };

            Assert.That(TelemetryHudConfigValidator.Validate(config), Has.Count.EqualTo(3));
        }
    }
}

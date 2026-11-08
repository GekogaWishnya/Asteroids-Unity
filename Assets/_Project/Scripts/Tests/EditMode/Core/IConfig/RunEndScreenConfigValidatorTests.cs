using System;
using NUnit.Framework;

namespace Asteroids.Core.IConfigs.Tests
{
    /// <summary>
    /// Titles are shown as written, so they only have to say something. The formats must fit what the screen passes:
    /// one int for the score, two ints (cleared, total) for the waves.
    /// </summary>
    [TestFixture]
    public sealed class RunEndScreenConfigValidatorTests
    {
        private sealed class TestScreenConfig : IRunEndScreenConfig
        {
            public string DefeatTitle { get; set; } = "GAME OVER";
            public string VictoryTitle { get; set; } = "STAGE COMPLETE";
            public string ScoreFormat { get; set; } = "SCORE {0:D6}";
            public string WavesFormat { get; set; } = "WAVES {0}/{1}";
        }

        [Test]
        public void Validate_Null_Throws() =>
            Assert.Throws<ArgumentNullException>(() => RunEndScreenConfigValidator.Validate(null));

        [Test]
        public void Validate_ShippedDefaults_ReportNothing() =>
            Assert.That(RunEndScreenConfigValidator.Validate(new TestScreenConfig()), Is.Empty);

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Validate_EmptyTitles_Report(string title)
        {
            var problems = RunEndScreenConfigValidator.Validate(
                new TestScreenConfig { DefeatTitle = title, VictoryTitle = title });

            Assert.That(problems, Has.Count.EqualTo(2));
            StringAssert.Contains("Defeat title", problems[0]);
            StringAssert.Contains("Victory title", problems[1]);
        }

        /// <summary>Titles are never formatted, so braces are just text.</summary>
        [Test]
        public void Validate_TitleWithBraces_ReportsNothing() =>
            Assert.That(RunEndScreenConfigValidator.Validate(new TestScreenConfig { DefeatTitle = "{oops}" }), Is.Empty);

        [TestCase(null)]
        [TestCase("")]
        [TestCase("{1}")]
        [TestCase("{0:D6")]
        public void Validate_BadScoreFormat_Reports(string format)
        {
            var problems = RunEndScreenConfigValidator.Validate(new TestScreenConfig { ScoreFormat = format });

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("Score format", problems[0]);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("{2}")]
        [TestCase("{0}/{1")]
        public void Validate_BadWavesFormat_Reports(string format)
        {
            var problems = RunEndScreenConfigValidator.Validate(new TestScreenConfig { WavesFormat = format });

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("Waves format", problems[0]);
        }

        /// <summary>Showing only one of the two values is a wording choice, not an error.</summary>
        [Test]
        public void Validate_WavesFormatUsingOnlyTheCleared_ReportsNothing() =>
            Assert.That(RunEndScreenConfigValidator.Validate(new TestScreenConfig { WavesFormat = "WAVE {0}" }), Is.Empty);
    }
}

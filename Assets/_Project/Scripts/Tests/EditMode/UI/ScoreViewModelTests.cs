using System;
using NUnit.Framework;

namespace Asteroids.UI.Tests
{
    /// <summary>
    /// The text follows the score. The wording and the zero padding come from <see cref="TestScoreHudConfig"/>
    /// ("{0}" unless a test sets its own), never from the shipped <c>HudConfig</c>.
    /// </summary>
    [TestFixture]
    public sealed class ScoreViewModelTests
    {
        private static ScoreViewModel Create(FakeScore score) => new(score, new TestScoreHudConfig());

        [Test]
        public void Text_StartsFromTheScoreAtConstruction()
        {
            var viewModel = Create(new FakeScore(42));

            Assert.That(viewModel.Text.Value, Is.EqualTo("42"));
        }

        [Test]
        public void Text_WhenScoreChanges_Follows()
        {
            var score = new FakeScore(0);
            var viewModel = Create(score);

            score.ValueSource.Value = 120;

            Assert.That(viewModel.Text.Value, Is.EqualTo("120"));
        }

        /// <summary>Padding is part of the format now, so a "D" specifier pads and a longer score is shown in full.</summary>
        [TestCase(7, "pts 0007")]
        [TestCase(12345678, "pts 12345678")]
        public void Text_PaddingFormat_PadsButNeverCuts(int value, string expected)
        {
            var viewModel = new ScoreViewModel(new FakeScore(value), new TestScoreHudConfig { ScoreFormat = "pts {0:D4}" });

            Assert.That(viewModel.Text.Value, Is.EqualTo(expected));
        }

        [Test]
        [SetCulture("de-DE")]
        public void Text_UnderAGroupingCulture_UsesTheInvariantCulture()
        {
            var viewModel = new ScoreViewModel(new FakeScore(1234567), new TestScoreHudConfig { ScoreFormat = "{0:N0}" });

            Assert.That(viewModel.Text.Value, Is.EqualTo("1,234,567"));
        }

        [Test]
        public void Dispose_StopsFollowingTheScore()
        {
            var score = new FakeScore(0);
            var viewModel = Create(score);

            viewModel.Dispose();
            score.ValueSource.Value = 500;

            Assert.That(viewModel.Text.Value, Is.EqualTo("0"));
        }

        [Test]
        public void Dispose_Twice_DoesNotThrow()
        {
            var viewModel = Create(new FakeScore(0));
            viewModel.Dispose();

            Assert.DoesNotThrow(() => viewModel.Dispose());
        }

        [Test]
        public void Constructor_NullScore_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ScoreViewModel(null, new TestScoreHudConfig()));
        }

        [Test]
        public void Constructor_NullConfig_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ScoreViewModel(new FakeScore(0), null));
        }

        /// <summary>A broken format fails at startup with the validator's message, not on the first kill.</summary>
        [Test]
        public void Constructor_InvalidConfig_Throws()
        {
            var config = new TestScoreHudConfig { ScoreFormat = "{0:D6" };

            var error = Assert.Throws<InvalidOperationException>(() => new ScoreViewModel(new FakeScore(0), config));
            StringAssert.Contains("Score format", error.Message);
        }
    }
}

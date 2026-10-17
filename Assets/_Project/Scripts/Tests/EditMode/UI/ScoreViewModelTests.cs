using System;
using NUnit.Framework;

namespace Asteroids.UI.Tests
{
    [TestFixture]
    public sealed class ScoreViewModelTests
    {
        [Test]
        public void Text_Initially_IsPaddedWithZeros()
        {
            var viewModel = new ScoreViewModel(new FakeScore(0));

            Assert.That(viewModel.Text.Value, Is.EqualTo("SCORE 000000"));
        }

        [Test]
        public void Text_WhenScoreChanges_Follows()
        {
            var score = new FakeScore(0);
            var viewModel = new ScoreViewModel(score);

            score.ValueSource.Value = 120;

            Assert.That(viewModel.Text.Value, Is.EqualTo("SCORE 000120"));
        }

        [Test]
        public void Text_ScoreLongerThanPadding_IsShownInFull()
        {
            var score = new FakeScore(0);
            var viewModel = new ScoreViewModel(score);

            score.ValueSource.Value = 12345678;

            Assert.That(viewModel.Text.Value, Is.EqualTo("SCORE 12345678"));
        }

        [Test]
        public void Text_StartsFromTheScoreAtConstruction()
        {
            var viewModel = new ScoreViewModel(new FakeScore(42));

            Assert.That(viewModel.Text.Value, Is.EqualTo("SCORE 000042"));
        }

        [Test]
        public void Dispose_StopsFollowingTheScore()
        {
            var score = new FakeScore(0);
            var viewModel = new ScoreViewModel(score);

            viewModel.Dispose();
            score.ValueSource.Value = 500;

            Assert.That(viewModel.Text.Value, Is.EqualTo("SCORE 000000"));
        }

        [Test]
        public void Dispose_Twice_DoesNotThrow()
        {
            var viewModel = new ScoreViewModel(new FakeScore(0));
            viewModel.Dispose();

            Assert.DoesNotThrow(() => viewModel.Dispose());
        }

        [Test]
        public void Constructor_NullScore_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ScoreViewModel(null));
        }
    }
}

using System;
using System.Collections.Generic;
using Asteroids.Core;
using NUnit.Framework;

namespace Asteroids.Scoring.Tests
{
    [TestFixture]
    public sealed class ScoreModelTests
    {
        [Test]
        public void New_StartsAtZero()
        {
            Assert.That(new ScoreModel().Value.Value, Is.EqualTo(0));
        }

        [Test]
        public void Add_SumsPoints()
        {
            var score = new ScoreModel();

            score.Add(100);
            score.Add(250);

            Assert.That(score.Value.Value, Is.EqualTo(350));
        }

        [Test]
        public void Add_NotifiesSubscribersWithEveryNewValue()
        {
            var score = new ScoreModel();
            var seen = new List<int>();
            score.Value.Subscribe(seen.Add);

            score.Add(10);
            score.Add(5);

            Assert.That(seen, Is.EqualTo(new[] { 0, 10, 15 }));
        }

        [TestCase(0)]
        [TestCase(-10)]
        public void Add_NonPositivePoints_ThrowsAndKeepsTheScore(int points)
        {
            var score = new ScoreModel();
            score.Add(7);

            Assert.Throws<ArgumentOutOfRangeException>(() => score.Add(points));
            Assert.That(score.Value.Value, Is.EqualTo(7));
        }

        [Test]
        public void Add_PastIntMax_SaturatesInsteadOfOverflowing()
        {
            var score = new ScoreModel();
            score.Add(int.MaxValue - 1);

            score.Add(10);

            Assert.That(score.Value.Value, Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void Add_WhenAlreadyAtMax_DoesNotNotify()
        {
            var score = new ScoreModel();
            score.Add(int.MaxValue);
            var seen = new List<int>();
            score.Value.Subscribe(seen.Add);
            seen.Clear();

            score.Add(1);

            Assert.That(seen, Is.Empty);
        }

        [Test]
        public void ReadAndWriteSides_ShareOneScore()
        {
            var model = new ScoreModel();
            IScore reader = model;
            IScoreSink sink = model;

            sink.Add(42);

            Assert.That(reader.Value.Value, Is.EqualTo(42));
        }
    }
}

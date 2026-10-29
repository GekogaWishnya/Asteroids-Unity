using System;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Enemies.Tests
{
    [TestFixture]
    public sealed class AwardScoreOnDeathTests
    {
        [Test]
        public void Constructor_NullScore_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new AwardScoreOnDeath(null, 10));
        }

        [TestCase(0)]
        [TestCase(-5)]
        public void Constructor_NonPositivePoints_Throws(int points)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AwardScoreOnDeath(new FakeScoreSink(), points));
        }

        [Test]
        public void Died_AwardsThePointsOnce()
        {
            var score = new FakeScoreSink();
            var handler = new AwardScoreOnDeath(score, 50);

            handler.Died(new Vector2(3f, 4f));

            Assert.That(score.Awarded, Is.EqualTo(new[] { 50 }));
        }

        [Test]
        public void Died_EveryDeath_AwardsAgain()
        {
            var score = new FakeScoreSink();
            var handler = new AwardScoreOnDeath(score, 50);

            handler.Died(Vector2.zero);
            handler.Died(Vector2.zero);

            Assert.That(score.Awarded, Is.EqualTo(new[] { 50, 50 }));
        }
    }
}

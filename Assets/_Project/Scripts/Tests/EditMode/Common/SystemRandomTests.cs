using System;
using NUnit.Framework;

namespace Asteroids.Common.Tests
{
    [TestFixture]
    public sealed class SystemRandomTests
    {
        private const int Samples = 2000;

        [Test]
        public void Value_Always_IsInUnitInterval()
        {
            var random = new SystemRandom(1);

            for (var i = 0; i < Samples; i++)
                Assert.That(random.Value(), Is.GreaterThanOrEqualTo(0f).And.LessThan(1f));
        }

        [Test]
        public void SameSeed_ProducesSameSequence()
        {
            var first = new SystemRandom(42);
            var second = new SystemRandom(42);

            for (var i = 0; i < 20; i++)
                Assert.That(second.Value(), Is.EqualTo(first.Value()));
        }

        [Test]
        public void RangeFloat_StaysWithinBounds()
        {
            var random = new SystemRandom(2);

            for (var i = 0; i < Samples; i++)
                Assert.That(random.Range(-3f, 5f), Is.InRange(-3f, 5f));
        }

        [Test]
        public void RangeFloat_EqualBounds_ReturnsThatValue()
        {
            Assert.That(new SystemRandom(3).Range(2.5f, 2.5f), Is.EqualTo(2.5f));
        }

        [Test]
        public void RangeFloat_MinAboveMax_Throws()
        {
            Assert.Throws<ArgumentException>(() => new SystemRandom(4).Range(2f, 1f));
        }

        [Test]
        public void RangeFloat_NonFiniteBound_Throws()
        {
            var random = new SystemRandom(5);

            Assert.Throws<ArgumentException>(() => random.Range(float.NaN, 1f));
            Assert.Throws<ArgumentException>(() => random.Range(0f, float.PositiveInfinity));
        }

        [Test]
        public void RangeInt_StaysInHalfOpenIntervalAndReachesEveryValue()
        {
            var random = new SystemRandom(6);
            var seen = new bool[4];

            for (var i = 0; i < Samples; i++)
            {
                var value = random.Range(0, 4);
                Assert.That(value, Is.InRange(0, 3));
                seen[value] = true;
            }

            Assert.That(seen, Is.All.True);
        }

        [Test]
        public void RangeInt_EmptyInterval_Throws()
        {
            var random = new SystemRandom(7);

            Assert.Throws<ArgumentOutOfRangeException>(() => random.Range(3, 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => random.Range(4, 3));
        }
    }
}

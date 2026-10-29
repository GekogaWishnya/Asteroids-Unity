using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Asteroids.Core.IConfigs.Tests
{
    [TestFixture]
    public sealed class LevelConfigValidatorTests
    {
        private static TestLevel LevelWith(params IWaveConfig[] waves) =>
            new() { Waves = new List<IWaveConfig>(waves) };

        private static TestWave WaveWith(params IWaveEntryConfig[] entries) =>
            new() { Entries = new List<IWaveEntryConfig>(entries) };

        [Test]
        public void Validate_NullLevel_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => LevelConfigValidator.Validate(null));
        }

        [Test]
        public void Validate_ValidLevel_ReportsNothing()
        {
            var level = LevelWith(
                new TestWave { DelayBeforeStart = 2f },
                WaveWith(
                    new TestWaveEntry { Count = 3, StartDelay = 1f, Interval = 0.5f },
                    new TestWaveEntry { Type = new EnemyTypeId("Ufo"), StartDelay = 20f }));

            Assert.That(LevelConfigValidator.Validate(level), Is.Empty);
        }

        [Test]
        public void Validate_NoWaves_Reports()
        {
            Assert.That(LevelConfigValidator.Validate(LevelWith()), Has.Count.EqualTo(1));
            Assert.That(LevelConfigValidator.Validate(new TestLevel { Waves = null }), Has.Count.EqualTo(1));
        }

        [Test]
        public void Validate_MissingWave_ReportsItsNumber()
        {
            var problems = LevelConfigValidator.Validate(LevelWith(new TestWave(), null));

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("Wave 2", problems[0]);
        }

        [Test]
        public void Validate_WaveWithoutEntries_ReportsItsNumber()
        {
            var problems = LevelConfigValidator.Validate(LevelWith(new TestWave(), WaveWith()));

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("Wave 2", problems[0]);
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Validate_BadDelayBeforeStart_Reports(float delay)
        {
            var problems = LevelConfigValidator.Validate(LevelWith(new TestWave { DelayBeforeStart = delay }));

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("Wave 1", problems[0]);
            StringAssert.Contains("delay before start", problems[0]);
        }

        [Test]
        public void Validate_MissingEntry_ReportsWaveAndEntryNumber()
        {
            var problems = LevelConfigValidator.Validate(LevelWith(WaveWith(new TestWaveEntry(), null)));

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("Wave 1, entry 2", problems[0]);
        }

        [Test]
        public void Validate_EntryWithoutType_Reports()
        {
            var problems = LevelConfigValidator.Validate(
                LevelWith(WaveWith(new TestWaveEntry { Type = default })));

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("no enemy type", problems[0]);
        }

        [TestCase(0)]
        [TestCase(-4)]
        public void Validate_CountBelowOne_Reports(int count)
        {
            var problems = LevelConfigValidator.Validate(
                LevelWith(WaveWith(new TestWaveEntry { Count = count })));

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("count", problems[0]);
        }

        [TestCase(-0.1f)]
        [TestCase(float.NaN)]
        public void Validate_BadStartDelay_Reports(float delay)
        {
            var problems = LevelConfigValidator.Validate(
                LevelWith(WaveWith(new TestWaveEntry { StartDelay = delay })));

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("start delay", problems[0]);
        }

        [TestCase(-0.1f)]
        [TestCase(float.NegativeInfinity)]
        public void Validate_BadInterval_Reports(float interval)
        {
            var problems = LevelConfigValidator.Validate(
                LevelWith(WaveWith(new TestWaveEntry { Interval = interval })));

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("interval", problems[0]);
        }

        [Test]
        public void Validate_SeveralMistakes_ReportsEachOfThem()
        {
            var problems = LevelConfigValidator.Validate(
                LevelWith(
                    WaveWith(new TestWaveEntry { Count = 0 }),
                    new TestWave { DelayBeforeStart = -1f, Entries = null }));

            Assert.That(problems, Has.Count.EqualTo(3));
        }
    }
}

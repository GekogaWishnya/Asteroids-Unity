using System;
using NUnit.Framework;
using static Asteroids.Waves.Tests.Build;

namespace Asteroids.Waves.Tests
{
    [TestFixture]
    public sealed class LevelValidatorTests
    {
        [Test]
        public void Validate_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => LevelValidator.Validate(null, new FakeEnemyData("A")));
            Assert.Throws<ArgumentNullException>(() => LevelValidator.Validate(Level(Wave(0f, Entry())), null));
        }

        [Test]
        public void Validate_PlayableLevel_ReportsNothing()
        {
            var level = Level(Wave(1f, Entry("A", 2), Entry("B", 1, start: 5f)));

            Assert.That(LevelValidator.Validate(level, new FakeEnemyData("A", "B")), Is.Empty);
        }

        [Test]
        public void Validate_TypeWithoutSpawner_ReportsWaveEntryAndType()
        {
            var level = Level(Wave(0f, Entry("A")), Wave(0f, Entry("A"), Entry("Ufo")));

            var problems = LevelValidator.Validate(level, new FakeEnemyData("A"));

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("Wave 2, entry 2", problems[0]);
            StringAssert.Contains("Ufo", problems[0]);
        }

        [Test]
        public void Validate_IncludesMistakesFoundInTheLevelAlone()
        {
            var level = Level(Wave(-1f, Entry("A", count: 0)));

            var problems = LevelValidator.Validate(level, new FakeEnemyData("A"));

            Assert.That(problems, Has.Count.EqualTo(2));
        }

        [Test]
        public void Validate_LevelWithoutWaves_DoesNotFail()
        {
            var problems = LevelValidator.Validate(Level(), new FakeEnemyData("A"));

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("no waves", problems[0]);
        }

        [Test]
        public void ThrowIfInvalid_ListsEveryProblem()
        {
            var level = Level(Wave(0f, Entry("Ghost")), Wave(0f, Entry("Phantom")));

            var ex = Assert.Throws<InvalidOperationException>(() =>
                LevelValidator.ThrowIfInvalid(level, new FakeEnemyData("A")));

            StringAssert.Contains("Ghost", ex.Message);
            StringAssert.Contains("Phantom", ex.Message);
        }

        [Test]
        public void ThrowIfInvalid_PlayableLevel_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                LevelValidator.ThrowIfInvalid(Level(Wave(0f, Entry("A"))), new FakeEnemyData("A")));
        }
    }
}

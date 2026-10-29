using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Asteroids.Core.IConfigs.Tests
{
    [TestFixture]
    public sealed class AsteroidConfigValidatorTests
    {
        private static TestAsteroidConfig ValidConfig() => new(
            new TestAsteroidType("AsteroidBig").SplitsInto("AsteroidMedium", 2),
            new TestAsteroidType("AsteroidMedium").SplitsInto("AsteroidSmall", 2),
            new TestAsteroidType("AsteroidSmall"));

        private static TestAsteroidConfig ConfigWith(TestAsteroidType type) => new(type);

        private static IReadOnlyList<string> Validate(params IAsteroidTypeConfig[] types) =>
            AsteroidConfigValidator.Validate(new TestAsteroidConfig(types));

        [Test]
        public void Validate_NullConfig_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => AsteroidConfigValidator.Validate(null));
        }

        [Test]
        public void Validate_ValidConfig_ReportsNothing()
        {
            Assert.That(AsteroidConfigValidator.Validate(ValidConfig()), Is.Empty);
        }

        [Test]
        public void Validate_NoTypes_Reports()
        {
            Assert.That(Validate(), Has.Count.EqualTo(1));
            Assert.That(AsteroidConfigValidator.Validate(new TestAsteroidConfig { Types = null }), Has.Count.EqualTo(1));
        }

        [Test]
        public void Validate_MissingType_Reports()
        {
            var problems = Validate(new TestAsteroidType("AsteroidBig"), null);

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("type 2", problems[0]);
        }

        [Test]
        public void Validate_TypeWithoutId_Reports()
        {
            var problems = Validate(new TestAsteroidType("AsteroidBig") { Type = default });

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("no type id", problems[0]);
        }

        [Test]
        public void Validate_DuplicateId_Reports()
        {
            var problems = Validate(new TestAsteroidType("AsteroidBig"), new TestAsteroidType("AsteroidBig"));

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("more than once", problems[0]);
        }

        [Test]
        public void Validate_MaxHealthBelowOne_Reports()
        {
            var problems = Validate(new TestAsteroidType("A") { MaxHealth = 0 });

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("max health", problems[0]);
        }

        [Test]
        public void Validate_ContactDamageBelowOne_Reports()
        {
            var problems = Validate(new TestAsteroidType("A") { ContactDamage = 0 });

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("contact damage", problems[0]);
        }

        [Test]
        public void Validate_ScorePointsZero_IsAllowed()
        {
            Assert.That(Validate(new TestAsteroidType("A") { ScorePoints = 0 }), Is.Empty);
        }

        [Test]
        public void Validate_NegativeScorePoints_Reports()
        {
            var problems = Validate(new TestAsteroidType("A") { ScorePoints = -5 });

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("score points", problems[0]);
        }

        [Test]
        public void Validate_SpeedMaxBelowMin_Reports()
        {
            var problems = Validate(new TestAsteroidType("A") { MinSpeed = 3f, MaxSpeed = 1f });

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("speed", problems[0]);
        }

        [Test]
        public void Validate_NegativeSpeed_ReportsWithoutComparingTheRange()
        {
            var problems = Validate(new TestAsteroidType("A") { MinSpeed = -1f, MaxSpeed = 2f });

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("minimum speed", problems[0]);
        }

        [Test]
        public void Validate_SpinSpeedMaxBelowMin_Reports()
        {
            var problems = Validate(new TestAsteroidType("A") { MinSpinSpeed = 50f, MaxSpinSpeed = 10f });

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("spin speed", problems[0]);
        }

        [Test]
        public void Validate_NonFiniteSpinSpeed_Reports()
        {
            var problems = Validate(new TestAsteroidType("A") { MaxSpinSpeed = float.NaN });

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("maximum spin speed", problems[0]);
        }

        [Test]
        public void Validate_ChildTypeWithoutCount_Reports()
        {
            var problems = Validate(
                new TestAsteroidType("A").SplitsInto("B", 0), new TestAsteroidType("B"));

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("child count is 0", problems[0]);
        }

        [Test]
        public void Validate_ChildCountWithoutType_Reports()
        {
            var problems = Validate(new TestAsteroidType("A") { ChildCount = 2 });

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("no child type", problems[0]);
        }

        [Test]
        public void Validate_NegativeChildCount_Reports()
        {
            var problems = Validate(
                new TestAsteroidType("A").SplitsInto("B", -1), new TestAsteroidType("B"));

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("child count must not be negative", problems[0]);
        }

        [Test]
        public void Validate_UnknownChildType_Reports()
        {
            var problems = Validate(new TestAsteroidType("A").SplitsInto("Ghost", 2));

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("'Ghost' is not defined", problems[0]);
        }

        [Test]
        public void Validate_SelfSplit_ReportsOneCycle()
        {
            var problems = Validate(new TestAsteroidType("A").SplitsInto("A", 2));

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("Split cycle: A -> A", problems[0]);
        }

        [Test]
        public void Validate_TwoTypeCycle_ReportsItOnce()
        {
            var problems = Validate(
                new TestAsteroidType("A").SplitsInto("B", 2),
                new TestAsteroidType("B").SplitsInto("A", 2));

            Assert.That(problems, Has.Exactly(1).Matches<string>(p => p.Contains("Split cycle")));
            Assert.That(problems, Has.Count.EqualTo(1));
        }

        [Test]
        public void Validate_ThreeTypeCycle_ReportsItOnce()
        {
            var problems = Validate(
                new TestAsteroidType("A").SplitsInto("B", 2),
                new TestAsteroidType("B").SplitsInto("C", 2),
                new TestAsteroidType("C").SplitsInto("A", 2));

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("A -> B -> C -> A", problems[0]);
        }

        [Test]
        public void Validate_ChainThatEntersACycle_ReportsTheCycleOnlyOnce()
        {
            var problems = Validate(
                new TestAsteroidType("D").SplitsInto("A", 2),
                new TestAsteroidType("A").SplitsInto("B", 2),
                new TestAsteroidType("B").SplitsInto("A", 2));

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("A -> B -> A", problems[0]);
        }

        [Test]
        public void Validate_PoolPrewarmAboveMaxIdle_Reports()
        {
            var problems = Validate(new TestAsteroidType("A") { PoolPrewarm = 10, PoolMaxIdle = 4 });

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("prewarm", problems[0]);
        }

        [Test]
        public void Validate_NegativePoolSizes_ReportEachOnce()
        {
            var problems = Validate(new TestAsteroidType("A") { PoolPrewarm = -1, PoolMaxIdle = -1 });

            Assert.That(problems, Has.Count.EqualTo(2));
        }

        [Test]
        public void Validate_SeveralMistakes_ReportsEachOfThem()
        {
            var problems = Validate(
                new TestAsteroidType("A") { MaxHealth = 0, ContactDamage = 0 },
                new TestAsteroidType("B") { MinSpeed = 5f, MaxSpeed = 1f });

            Assert.That(problems, Has.Count.EqualTo(3));
        }
    }
}

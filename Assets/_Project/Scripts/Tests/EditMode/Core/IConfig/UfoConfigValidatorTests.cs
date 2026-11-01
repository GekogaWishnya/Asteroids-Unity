using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Asteroids.Core.IConfigs.Tests
{
    [TestFixture]
    public sealed class UfoConfigValidatorTests
    {
        private static IReadOnlyList<string> Validate(params IUfoTypeConfig[] types) =>
            UfoConfigValidator.Validate(new TestUfoConfig(types));

        private static void AssertSingleProblem(TestUfoType type, string fragment)
        {
            var problems = Validate(type);

            Assert.That(problems, Has.Count.EqualTo(1), string.Join("\n", problems));
            StringAssert.Contains(fragment, problems[0]);
        }

        [Test]
        public void Validate_NullConfig_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => UfoConfigValidator.Validate(null));
        }

        [Test]
        public void Validate_ValidConfig_ReportsNothing()
        {
            Assert.That(Validate(new TestUfoType("Ufo"), new TestUfoType("UfoFast")), Is.Empty);
        }

        [Test]
        public void Validate_NoTypes_Reports()
        {
            Assert.That(Validate(), Has.Count.EqualTo(1));
            Assert.That(UfoConfigValidator.Validate(new TestUfoConfig { Types = null }), Has.Count.EqualTo(1));
        }

        [Test]
        public void Validate_MissingType_Reports()
        {
            var problems = Validate(new TestUfoType(), null);

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("type 2", problems[0]);
        }

        [Test]
        public void Validate_TypeWithoutId_Reports()
        {
            AssertSingleProblem(new TestUfoType { Type = default }, "no type id");
        }

        [Test]
        public void Validate_DuplicateId_ReportsOnce()
        {
            var problems = Validate(new TestUfoType("Ufo"), new TestUfoType("Ufo") { MaxHealth = 0 });

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("more than once", problems[0]);
        }

        [Test]
        public void Validate_MaxHealthBelowOne_Reports() =>
            AssertSingleProblem(new TestUfoType { MaxHealth = 0 }, "max health");

        [Test]
        public void Validate_ContactDamageBelowOne_Reports() =>
            AssertSingleProblem(new TestUfoType { ContactDamage = 0 }, "contact damage");

        [Test]
        public void Validate_NegativePoints_Reports() =>
            AssertSingleProblem(new TestUfoType { ScorePoints = -1 }, "score points");

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Validate_MaxSpeedNotPositiveAndFinite_Reports(float maxSpeed) =>
            AssertSingleProblem(new TestUfoType { MaxSpeed = maxSpeed, LaunchSpeed = 0f }, "max speed");

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        public void Validate_NegativeOrNaNTuning_ReportsEachValue(float value)
        {
            AssertSingleProblem(new TestUfoType { LaunchSpeed = value }, "launch speed");
            AssertSingleProblem(new TestUfoType { Thrust = value }, "thrust");
            AssertSingleProblem(new TestUfoType { TurnSpeed = value }, "turn speed");
            AssertSingleProblem(new TestUfoType { LinearDamping = value }, "linear damping");
            AssertSingleProblem(new TestUfoType { MinKnockbackSpeed = value }, "min knockback speed");
            AssertSingleProblem(new TestUfoType { StunSeconds = value }, "stun seconds");
        }

        [Test]
        public void Validate_LaunchSpeedAboveMaxSpeed_Reports() =>
            AssertSingleProblem(new TestUfoType { LaunchSpeed = 6f, MaxSpeed = 5f }, "above max speed");

        [Test]
        public void Validate_PoolSettings_UseTheSharedPoolRules()
        {
            AssertSingleProblem(new TestUfoType { PoolPrewarm = -1 }, "pool prewarm must not be negative");
            AssertSingleProblem(new TestUfoType { PoolMaxIdle = -1, PoolPrewarm = 0 }, "pool max idle");
            AssertSingleProblem(new TestUfoType { PoolPrewarm = 9, PoolMaxIdle = 8 }, "above max idle");
        }

        [Test]
        public void Validate_SeveralProblems_ReportsAllOfThem()
        {
            var problems = Validate(new TestUfoType { MaxHealth = 0, ContactDamage = 0, Thrust = -1f });

            Assert.That(problems, Has.Count.EqualTo(3));
        }
    }
}

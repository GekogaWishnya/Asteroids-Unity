using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Asteroids.Core.IConfigs.Tests
{
    [TestFixture]
    public sealed class EffectsConfigValidatorTests
    {
        private sealed class Effect : IEffectConfig
        {
            public string Name { get; set; } = "Sparks";
            public float Scale { get; set; } = 1f;
            public int PoolPrewarm { get; set; } = 2;
            public int PoolMaxIdle { get; set; } = 4;
            public int MaxActive { get; set; } = 8;
        }

        private class Feedback : IFeedbackConfig
        {
            public IEffectConfig Hit { get; set; }
            public bool PlayHitOnFatal { get; set; }
            public IEffectConfig Death { get; set; } = new Effect { Name = "Boom" };
        }

        private sealed class EnemyFeedback : Feedback, IEnemyFeedbackConfig
        {
            public EnemyFeedback(string type) => Type = type == null ? default : new EnemyTypeId(type);

            public EnemyTypeId Type { get; }
        }

        private sealed class Config : IEffectsConfig
        {
            public IFeedbackConfig Ship { get; set; } = new Feedback();
            public IReadOnlyList<IEnemyFeedbackConfig> Enemies { get; set; } = new List<IEnemyFeedbackConfig>();
        }

        [Test]
        public void Validate_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => EffectsConfigValidator.Validate(null));
            Assert.Throws<ArgumentNullException>(() =>
                EffectsConfigValidator.ValidateFeedback("Ship", null, new List<string>()));
            Assert.Throws<ArgumentNullException>(() =>
                EffectsConfigValidator.ValidateFeedback("Ship", new Feedback(), null));
            Assert.Throws<ArgumentNullException>(() =>
                EffectsConfigValidator.ValidateEffect("Sparks", null, new List<string>()));
        }

        [Test]
        public void Validate_CompleteConfig_ReportsNothing()
        {
            var config = new Config
            {
                Ship = new Feedback { Hit = new Effect() },
                Enemies = new List<IEnemyFeedbackConfig> { new EnemyFeedback("A"), new EnemyFeedback("B") }
            };

            Assert.That(EffectsConfigValidator.Validate(config), Is.Empty);
        }

        [Test]
        public void Validate_MissingShip_IsReported_ButMissingEffectsAreNot()
        {
            // Hit and death effects are both optional; only the ship's whole entry must exist.
            var config = new Config
            {
                Ship = null,
                Enemies = new List<IEnemyFeedbackConfig> { new EnemyFeedback("A") { Death = null } }
            };

            var problems = EffectsConfigValidator.Validate(config);

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("ship", problems[0]);
        }

        [Test]
        public void Validate_BadEntries_ReportsMissingInvalidAndDuplicate()
        {
            var config = new Config
            {
                Enemies = new List<IEnemyFeedbackConfig>
                {
                    null, new EnemyFeedback(null), new EnemyFeedback("A"), new EnemyFeedback("A")
                }
            };

            var problems = EffectsConfigValidator.Validate(config);

            Assert.That(problems, Has.Count.EqualTo(3));
            StringAssert.Contains("entry 1 is missing", problems[0]);
            StringAssert.Contains("entry 2 has no enemy type", problems[1]);
            StringAssert.Contains("'A' has more than one", problems[2]);
        }

        [Test]
        public void ValidateFeedback_PrefixesEachProblemWithTheLabel()
        {
            // Assets validate their own effects with this, so the label is what the designer reads first.
            var problems = new List<string>();

            EffectsConfigValidator.ValidateFeedback(
                "Effects",
                new Feedback { Hit = new Effect { Scale = -1f }, Death = new Effect { Name = "Boom", MaxActive = 0 } },
                problems);

            Assert.That(problems, Has.Count.EqualTo(2));
            StringAssert.StartsWith("Effects, death effect 'Boom': at least one", problems[0]);
            StringAssert.StartsWith("Effects, hit effect 'Sparks': scale", problems[1]);
        }

        [Test]
        public void Validate_BadEffectSettings_ReportsScaleMaxActiveAndPool()
        {
            var config = new Config
            {
                Ship = new Feedback
                {
                    Hit = new Effect { Scale = 0f, MaxActive = 0, PoolPrewarm = 5, PoolMaxIdle = 1 }
                }
            };

            var problems = EffectsConfigValidator.Validate(config);

            Assert.That(problems, Has.Count.EqualTo(3));
            StringAssert.Contains("scale", problems[0]);
            StringAssert.Contains("max active", problems[1]);
            StringAssert.Contains("prewarm", problems[2]);
        }

        [Test]
        public void Validate_NoHitEffect_IsFine()
        {
            Assert.That(EffectsConfigValidator.Validate(new Config { Ship = new Feedback { PlayHitOnFatal = true } }),
                Is.Empty);
        }
    }
}

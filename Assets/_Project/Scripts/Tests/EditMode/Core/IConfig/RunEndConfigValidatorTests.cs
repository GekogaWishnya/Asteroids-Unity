using System;
using NUnit.Framework;

namespace Asteroids.Core.IConfigs.Tests
{
    /// <summary>Both end delays must be finite and not negative. 0 is allowed and means "end at once".</summary>
    [TestFixture]
    public sealed class RunEndConfigValidatorTests
    {
        private sealed class TestRunEndConfig : IRunEndConfig
        {
            public float DefeatDelay { get; set; } = 1.5f;
            public float VictoryDelay { get; set; } = 1f;
        }

        [Test]
        public void Validate_Null_Throws() =>
            Assert.Throws<ArgumentNullException>(() => RunEndConfigValidator.Validate(null));

        [Test]
        public void Validate_ShippedDefaults_ReportNothing() =>
            Assert.That(RunEndConfigValidator.Validate(new TestRunEndConfig()), Is.Empty);

        [Test]
        public void Validate_ZeroDelays_ReportNothing() =>
            Assert.That(RunEndConfigValidator.Validate(new TestRunEndConfig { DefeatDelay = 0f, VictoryDelay = 0f }),
                Is.Empty);

        [TestCase(-0.1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Validate_BadDefeatDelay_Reports(float delay)
        {
            var problems = RunEndConfigValidator.Validate(new TestRunEndConfig { DefeatDelay = delay });

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("Defeat delay", problems[0]);
        }

        [TestCase(-0.1f)]
        [TestCase(float.NaN)]
        [TestCase(float.NegativeInfinity)]
        public void Validate_BadVictoryDelay_Reports(float delay)
        {
            var problems = RunEndConfigValidator.Validate(new TestRunEndConfig { VictoryDelay = delay });

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("Victory delay", problems[0]);
        }

        [Test]
        public void Validate_BothBad_ReportsBoth() =>
            Assert.That(RunEndConfigValidator.Validate(new TestRunEndConfig { DefeatDelay = -1f, VictoryDelay = -1f }),
                Has.Count.EqualTo(2));
    }
}

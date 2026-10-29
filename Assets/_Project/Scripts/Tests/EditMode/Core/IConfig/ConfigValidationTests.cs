using System;
using NUnit.Framework;

namespace Asteroids.Core.IConfigs.Tests
{
    [TestFixture]
    public sealed class ConfigValidationTests
    {
        [Test]
        public void ThrowIfInvalid_NoProblems_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => ConfigValidation.ThrowIfInvalid("The level", Array.Empty<string>()));
        }

        [Test]
        public void ThrowIfInvalid_WithProblems_ThrowsListingAllOfThem()
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                ConfigValidation.ThrowIfInvalid("The level", new[] { "first problem", "second problem" }));

            StringAssert.Contains("The level is invalid", ex.Message);
            StringAssert.Contains("2 problem(s)", ex.Message);
            StringAssert.Contains("first problem", ex.Message);
            StringAssert.Contains("second problem", ex.Message);
        }

        [Test]
        public void ThrowIfInvalid_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => ConfigValidation.ThrowIfInvalid(null, Array.Empty<string>()));
            Assert.Throws<ArgumentNullException>(() => ConfigValidation.ThrowIfInvalid("The level", null));
        }
    }
}

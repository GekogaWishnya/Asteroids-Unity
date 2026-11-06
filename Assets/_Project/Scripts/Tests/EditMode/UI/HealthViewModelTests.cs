using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Asteroids.UI.Tests
{
    /// <summary>
    /// The text follows the health and notifies once per visible change. The wording comes from
    /// <see cref="TestHealthHudConfig"/> ("{0}/{1}"), never from the shipped <c>HudConfig</c>.
    /// </summary>
    [TestFixture]
    public sealed class HealthViewModelTests
    {
        private static HealthViewModel Create(FakePlayerHealth health) => new(health, new TestHealthHudConfig());

        [Test]
        public void Text_Initially_ShowsCurrentOverMax()
        {
            var viewModel = Create(new FakePlayerHealth(3, 3));

            Assert.That(viewModel.Text.Value, Is.EqualTo("3/3"));
        }

        [Test]
        public void Text_UsesTheConfiguredFormat()
        {
            var config = new TestHealthHudConfig { HealthFormat = "life {0} of {1}" };

            var viewModel = new HealthViewModel(new FakePlayerHealth(2, 4), config);

            Assert.That(viewModel.Text.Value, Is.EqualTo("life 2 of 4"));
        }

        [Test]
        public void Text_WhenHealthChanges_Follows()
        {
            var health = new FakePlayerHealth(3, 3);
            var viewModel = Create(health);

            health.CurrentValue.Value = 1;

            Assert.That(viewModel.Text.Value, Is.EqualTo("1/3"));
        }

        [Test]
        public void Text_WhenMaxChanges_Follows()
        {
            var health = new FakePlayerHealth(3, 3);
            var viewModel = Create(health);

            health.MaxValue.Value = 5;

            Assert.That(viewModel.Text.Value, Is.EqualTo("3/5"));
        }

        [Test]
        public void Text_WhenViewSubscribesLate_GetsTheCurrentTextAtOnce()
        {
            var health = new FakePlayerHealth(3, 3);
            var viewModel = Create(health);
            health.CurrentValue.Value = 2;
            var seen = new List<string>();

            viewModel.Text.Subscribe(seen.Add);

            Assert.That(seen, Is.EqualTo(new[] { "2/3" }));
        }

        [Test]
        public void Text_NotifiesOncePerVisibleChange()
        {
            var health = new FakePlayerHealth(3, 3);
            var viewModel = Create(health);
            var seen = new List<string>();
            viewModel.Text.Subscribe(seen.Add);
            seen.Clear();

            health.CurrentValue.Value = 2;
            health.CurrentValue.Value = 2;

            Assert.That(seen, Is.EqualTo(new[] { "2/3" }));
        }

        [Test]
        public void CurrentAndMax_ArePassedThroughFromTheHealth()
        {
            var health = new FakePlayerHealth(3, 5);
            var viewModel = Create(health);

            Assert.That(viewModel.Current, Is.SameAs(health.Current));
            Assert.That(viewModel.Max, Is.SameAs(health.Max));
        }

        [Test]
        public void Dispose_StopsFollowingTheHealth()
        {
            var health = new FakePlayerHealth(3, 3);
            var viewModel = Create(health);

            viewModel.Dispose();
            health.CurrentValue.Value = 0;
            health.MaxValue.Value = 9;

            Assert.That(viewModel.Text.Value, Is.EqualTo("3/3"));
        }

        [Test]
        public void Dispose_Twice_DoesNotThrow()
        {
            var viewModel = Create(new FakePlayerHealth(3, 3));
            viewModel.Dispose();

            Assert.DoesNotThrow(() => viewModel.Dispose());
        }

        [Test]
        public void Constructor_NullHealth_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new HealthViewModel(null, new TestHealthHudConfig()));
        }

        [Test]
        public void Constructor_NullConfig_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new HealthViewModel(new FakePlayerHealth(3, 3), null));
        }

        /// <summary>A broken format fails at startup with the validator's message, not on the first hit.</summary>
        [Test]
        public void Constructor_InvalidConfig_Throws()
        {
            var config = new TestHealthHudConfig { HealthFormat = "{2}" };

            var error = Assert.Throws<InvalidOperationException>(() =>
                new HealthViewModel(new FakePlayerHealth(3, 3), config));
            StringAssert.Contains("Health format", error.Message);
        }
    }
}

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Asteroids.UI.Tests
{
    [TestFixture]
    public sealed class HealthViewModelTests
    {
        [Test]
        public void Text_Initially_ShowsCurrentOverMax()
        {
            var viewModel = new HealthViewModel(new FakePlayerHealth(3, 3));

            Assert.That(viewModel.Text.Value, Is.EqualTo("HP 3/3"));
        }

        [Test]
        public void Text_WhenHealthChanges_Follows()
        {
            var health = new FakePlayerHealth(3, 3);
            var viewModel = new HealthViewModel(health);

            health.CurrentValue.Value = 1;

            Assert.That(viewModel.Text.Value, Is.EqualTo("HP 1/3"));
        }

        [Test]
        public void Text_WhenMaxChanges_Follows()
        {
            var health = new FakePlayerHealth(3, 3);
            var viewModel = new HealthViewModel(health);

            health.MaxValue.Value = 5;

            Assert.That(viewModel.Text.Value, Is.EqualTo("HP 3/5"));
        }

        [Test]
        public void Text_WhenViewSubscribesLate_GetsTheCurrentTextAtOnce()
        {
            var health = new FakePlayerHealth(3, 3);
            var viewModel = new HealthViewModel(health);
            health.CurrentValue.Value = 2;
            var seen = new List<string>();

            viewModel.Text.Subscribe(seen.Add);

            Assert.That(seen, Is.EqualTo(new[] { "HP 2/3" }));
        }

        [Test]
        public void Text_NotifiesOncePerVisibleChange()
        {
            var health = new FakePlayerHealth(3, 3);
            var viewModel = new HealthViewModel(health);
            var seen = new List<string>();
            viewModel.Text.Subscribe(seen.Add);
            seen.Clear();

            health.CurrentValue.Value = 2;
            health.CurrentValue.Value = 2;

            Assert.That(seen, Is.EqualTo(new[] { "HP 2/3" }));
        }

        [Test]
        public void CurrentAndMax_ArePassedThroughFromTheHealth()
        {
            var health = new FakePlayerHealth(3, 5);
            var viewModel = new HealthViewModel(health);

            Assert.That(viewModel.Current, Is.SameAs(health.Current));
            Assert.That(viewModel.Max, Is.SameAs(health.Max));
        }

        [Test]
        public void Dispose_StopsFollowingTheHealth()
        {
            var health = new FakePlayerHealth(3, 3);
            var viewModel = new HealthViewModel(health);

            viewModel.Dispose();
            health.CurrentValue.Value = 0;
            health.MaxValue.Value = 9;

            Assert.That(viewModel.Text.Value, Is.EqualTo("HP 3/3"));
        }

        [Test]
        public void Dispose_Twice_DoesNotThrow()
        {
            var viewModel = new HealthViewModel(new FakePlayerHealth(3, 3));
            viewModel.Dispose();

            Assert.DoesNotThrow(() => viewModel.Dispose());
        }

        [Test]
        public void Constructor_NullHealth_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new HealthViewModel(null));
        }
    }
}

using System;
using Asteroids.Core.IConfigs;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Enemies.Tests
{
    [TestFixture]
    public sealed class SplitOnDeathTests
    {
        private static readonly EnemyTypeId Child = new("AsteroidMedium");

        [Test]
        public void Constructor_NullLauncher_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new SplitOnDeath(null, Child, 2));
        }

        [Test]
        public void Constructor_DefaultChildType_Throws()
        {
            Assert.Throws<ArgumentException>(() => new SplitOnDeath(new RecordingLauncher(), default, 2));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_NoChildren_Throws(int count)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SplitOnDeath(new RecordingLauncher(), Child, count));
        }

        [Test]
        public void Died_LaunchesEveryChildAtThePlaceOfDeath()
        {
            var launcher = new RecordingLauncher();
            var handler = new SplitOnDeath(launcher, Child, 3);

            handler.Died(new Vector2(1f, 2f));

            Assert.That(launcher.Launches, Has.Count.EqualTo(3));
            foreach (var (type, position) in launcher.Launches)
            {
                Assert.That(type, Is.EqualTo(Child));
                Assert.That(position, Is.EqualTo(new Vector2(1f, 2f)));
            }
        }

        [Test]
        public void Died_EveryDeath_LaunchesAgain()
        {
            var launcher = new RecordingLauncher();
            var handler = new SplitOnDeath(launcher, Child, 2);

            handler.Died(Vector2.zero);
            handler.Died(Vector2.one);

            Assert.That(launcher.Launches, Has.Count.EqualTo(4));
        }
    }
}

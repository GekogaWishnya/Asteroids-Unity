using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Asteroids.Core.IConfigs.Tests
{
    /// <summary><see cref="AsteroidConfig"/> and <see cref="UfoConfig"/>, the configs gathered from the level.</summary>
    [TestFixture]
    public sealed class EnemyConfigsTests
    {
        [Test]
        public void Constructors_NullTypes_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new AsteroidConfig(null));
            Assert.Throws<ArgumentNullException>(() => new UfoConfig(null));
        }

        [Test]
        public void AsteroidConfig_KeepsTheTypesInOrder_AndCopiesTheSource()
        {
            var big = new TestAsteroidType("Big");
            var small = new TestAsteroidType("Small");
            var source = new List<IAsteroidTypeConfig> { big, small };

            var config = new AsteroidConfig(source);
            source.Clear();

            Assert.That(config.Types, Is.EqualTo(new IAsteroidTypeConfig[] { big, small }));
        }

        [Test]
        public void UfoConfig_KeepsTheTypesInOrder_AndCopiesTheSource()
        {
            var ufo = new TestUfoType("Ufo");
            var source = new List<IUfoTypeConfig> { ufo };

            var config = new UfoConfig(source);
            source.Clear();

            Assert.That(config.Types, Is.EqualTo(new IUfoTypeConfig[] { ufo }));
        }

        [Test]
        public void EmptyConfigs_AreValid()
        {
            // A level with only asteroids yields an empty UFO config and the other way round.
            Assert.That(AsteroidConfigValidator.Validate(new AsteroidConfig(new IAsteroidTypeConfig[0])), Is.Empty);
            Assert.That(UfoConfigValidator.Validate(new UfoConfig(new IUfoTypeConfig[0])), Is.Empty);
        }
    }
}

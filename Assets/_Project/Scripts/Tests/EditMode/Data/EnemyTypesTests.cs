using System;
using NUnit.Framework;

namespace Asteroids.Data.Tests
{
    [TestFixture]
    public sealed class EnemyTypesTests
    {
        private TestAssets _assets;

        [SetUp]
        public void SetUp() => _assets = new TestAssets();

        [TearDown]
        public void TearDown() => _assets.DestroyAll();

        [Test]
        public void Collect_NullSources_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => EnemyTypes.Collect(null));
            Assert.Throws<ArgumentNullException>(() => EnemyTypes.Collect(new IEnemyTypeSource[] { null }));
        }

        [Test]
        public void Collect_ReturnsEachTypeOnce_InTheOrderFirstMet_AndSkipsEmptyEntries()
        {
            // An asteroid config refers to the medium type as a child and as its own entry, and the UFO config
            // is unrelated: the medium type must still come out once, where it was first met.
            var big = _assets.EnemyType("Big");
            var medium = _assets.EnemyType("Medium");
            var ufo = _assets.EnemyType("Ufo");

            var types = EnemyTypes.Collect(new IEnemyTypeSource[]
            {
                new TestTypeSource(big, medium, null, medium),
                new TestTypeSource(ufo, big)
            });

            Assert.That(types, Is.EqualTo(new[] { big, medium, ufo }));
        }

        [Test]
        public void Collect_TwoTypesWithOneName_Throws()
        {
            var first = _assets.EnemyType("Rock");
            var second = _assets.EnemyType("Rock");

            var error = Assert.Throws<InvalidOperationException>(() =>
                EnemyTypes.Collect(new IEnemyTypeSource[] { new TestTypeSource(first), new TestTypeSource(second) }));

            StringAssert.Contains("'Rock'", error.Message);
        }

        [Test]
        public void Collect_UnnamedType_Throws()
        {
            var unnamed = _assets.EnemyType(" ");

            Assert.Throws<InvalidOperationException>(() =>
                EnemyTypes.Collect(new IEnemyTypeSource[] { new TestTypeSource(unnamed) }));
        }
    }
}

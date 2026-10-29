using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Asteroids.Core.IConfigs.Tests
{
    [TestFixture]
    public sealed class EnemyTypeIdTests
    {
        [Test]
        public void Constructor_WithText_IsValidAndKeepsValue()
        {
            var id = new EnemyTypeId("Ufo");

            Assert.That(id.IsValid, Is.True);
            Assert.That(id.Value, Is.EqualTo("Ufo"));
            Assert.That(id.ToString(), Is.EqualTo("Ufo"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Constructor_NullEmptyOrWhiteSpace_Throws(string value)
        {
            Assert.Throws<ArgumentException>(() => new EnemyTypeId(value));
        }

        [Test]
        public void Default_IsNotValid()
        {
            var id = default(EnemyTypeId);

            Assert.That(id.IsValid, Is.False);
            Assert.That(id.Value, Is.Null);
            Assert.That(id.ToString(), Is.EqualTo("<none>"));
        }

        [Test]
        public void Equality_SameText_IsEqualWithEqualHashCode()
        {
            var first = new EnemyTypeId("AsteroidBig");
            var second = new EnemyTypeId(new string("AsteroidBig".ToCharArray()));

            Assert.That(first, Is.EqualTo(second));
            Assert.That(first == second, Is.True);
            Assert.That(first != second, Is.False);
            Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
        }

        [Test]
        public void Equality_DifferentCase_IsNotEqual()
        {
            Assert.That(new EnemyTypeId("Ufo"), Is.Not.EqualTo(new EnemyTypeId("ufo")));
        }

        [Test]
        public void Equality_DefaultAgainstDefault_IsEqual()
        {
            Assert.That(default(EnemyTypeId) == default, Is.True);
            Assert.That(default(EnemyTypeId).GetHashCode(), Is.EqualTo(0));
        }

        [Test]
        public void Equality_DefaultAgainstValid_IsNotEqual()
        {
            Assert.That(default(EnemyTypeId) == new EnemyTypeId("Ufo"), Is.False);
        }

        [Test]
        public void AsDictionaryKey_FindsEqualId()
        {
            var map = new Dictionary<EnemyTypeId, int> { [new EnemyTypeId("Ufo")] = 7 };

            Assert.That(map[new EnemyTypeId("Ufo")], Is.EqualTo(7));
        }
    }
}

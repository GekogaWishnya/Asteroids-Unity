using System.Linq;
using Asteroids.Core.IConfigs;
using NUnit.Framework;
using UnityEditor;

namespace Asteroids.Data.Tests
{
    /// <summary>
    /// The asteroid and UFO type assets are what the enemy features read now, so these tests check that what the
    /// inspector writes reaches the config interfaces unchanged. Fields are written through
    /// <see cref="SerializedObject"/>, by their serialized names, so a renamed field breaks a test here instead of
    /// silently losing data in the designer's assets.
    /// </summary>
    [TestFixture]
    public sealed class EnemyTypeAssetTests
    {
        private TestAssets _assets;

        [SetUp]
        public void SetUp() => _assets = new TestAssets();

        [TearDown]
        public void TearDown() => _assets.DestroyAll();

        [Test]
        public void Asteroid_ExposesItsSettingsAsAnAsteroidTypeConfig()
        {
            var small = _assets.Asteroid("AsteroidSmall");
            var big = _assets.Asteroid("AsteroidBig", small, 2);
            var serialized = new SerializedObject(big);
            serialized.FindProperty("maxHealth").intValue = 3;
            serialized.FindProperty("contactDamage").intValue = 2;
            serialized.FindProperty("scorePoints").intValue = 20;
            serialized.FindProperty("minSpeed").floatValue = 0.8f;
            serialized.FindProperty("maxSpeed").floatValue = 1.6f;
            serialized.FindProperty("minSpinSpeed").floatValue = 15f;
            serialized.FindProperty("maxSpinSpeed").floatValue = 40f;
            serialized.FindProperty("poolPrewarm").intValue = 8;
            serialized.FindProperty("poolMaxIdle").intValue = 16;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            IAsteroidTypeConfig config = big;

            Assert.That(config.Type, Is.EqualTo(new EnemyTypeId("AsteroidBig")));
            Assert.That(config.MaxHealth, Is.EqualTo(3));
            Assert.That(config.ContactDamage, Is.EqualTo(2));
            Assert.That(config.ScorePoints, Is.EqualTo(20));
            Assert.That(config.MinSpeed, Is.EqualTo(0.8f));
            Assert.That(config.MaxSpeed, Is.EqualTo(1.6f));
            Assert.That(config.MinSpinSpeed, Is.EqualTo(15f));
            Assert.That(config.MaxSpinSpeed, Is.EqualTo(40f));
            Assert.That(config.ChildType, Is.EqualTo(new EnemyTypeId("AsteroidSmall")));
            Assert.That(config.ChildCount, Is.EqualTo(2));
            Assert.That(config.PoolPrewarm, Is.EqualTo(8));
            Assert.That(config.PoolMaxIdle, Is.EqualTo(16));
            Assert.That(big.LinkedTypes, Is.EqualTo(new EnemyTypeDefinition[] { small }));
        }

        [Test]
        public void Asteroid_WithoutAChild_HasNoChildTypeAndNoLinks()
        {
            var small = _assets.Asteroid("AsteroidSmall");

            Assert.That(((IAsteroidTypeConfig)small).ChildType.IsValid, Is.False);
            Assert.That(small.LinkedTypes, Is.Empty);
        }

        [Test]
        public void Ufo_ExposesItsSettingsAsAUfoTypeConfig()
        {
            var ufo = _assets.Ufo("Ufo");
            var serialized = new SerializedObject(ufo);
            serialized.FindProperty("maxHealth").intValue = 2;
            serialized.FindProperty("contactDamage").intValue = 1;
            serialized.FindProperty("scorePoints").intValue = 150;
            serialized.FindProperty("launchSpeed").floatValue = 2f;
            serialized.FindProperty("thrust").floatValue = 6f;
            serialized.FindProperty("turnSpeed").floatValue = 120f;
            serialized.FindProperty("maxSpeed").floatValue = 5f;
            serialized.FindProperty("linearDamping").floatValue = 0.5f;
            serialized.FindProperty("minKnockbackSpeed").floatValue = 3f;
            serialized.FindProperty("stunSeconds").floatValue = 1.5f;
            serialized.FindProperty("poolPrewarm").intValue = 4;
            serialized.FindProperty("poolMaxIdle").intValue = 8;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            IUfoTypeConfig config = ufo;

            Assert.That(config.Type, Is.EqualTo(new EnemyTypeId("Ufo")));
            Assert.That(config.MaxHealth, Is.EqualTo(2));
            Assert.That(config.ContactDamage, Is.EqualTo(1));
            Assert.That(config.ScorePoints, Is.EqualTo(150));
            Assert.That(config.LaunchSpeed, Is.EqualTo(2f));
            Assert.That(config.Thrust, Is.EqualTo(6f));
            Assert.That(config.TurnSpeed, Is.EqualTo(120f));
            Assert.That(config.MaxSpeed, Is.EqualTo(5f));
            Assert.That(config.LinearDamping, Is.EqualTo(0.5f));
            Assert.That(config.MinKnockbackSpeed, Is.EqualTo(3f));
            Assert.That(config.StunSeconds, Is.EqualTo(1.5f));
            Assert.That(config.PoolPrewarm, Is.EqualTo(4));
            Assert.That(config.PoolMaxIdle, Is.EqualTo(8));
            Assert.That(ufo.LinkedTypes, Is.Empty);
        }

        [Test]
        public void Level_RefersToTheTypesOfItsWaveEntries()
        {
            var big = _assets.Asteroid("AsteroidBig");
            var ufo = _assets.Ufo("Ufo");
            var level = _assets.Create<LevelConfig>("Level");
            var serialized = new SerializedObject(level);
            var waves = serialized.FindProperty("waves");
            waves.arraySize = 2;
            SetEntries(waves.GetArrayElementAtIndex(0), big);
            SetEntries(waves.GetArrayElementAtIndex(1), big, ufo);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            // Repeats are expected here; EnemyTypes.Collect removes them.
            Assert.That(level.ReferencedTypes.ToArray(), Is.EqualTo(new EnemyTypeDefinition[] { big, big, ufo }));
        }

        private static void SetEntries(SerializedProperty wave, params EnemyTypeDefinition[] types)
        {
            var entries = wave.FindPropertyRelative("entries");
            entries.arraySize = types.Length;
            for (var i = 0; i < types.Length; i++)
            {
                // Elements added through arraySize are zeroed, not built from the field initializers,
                // so count starts at 0 here and LevelConfig.OnValidate would warn about it.
                var entry = entries.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("type").objectReferenceValue = types[i];
                entry.FindPropertyRelative("count").intValue = 1;
            }
        }
    }
}
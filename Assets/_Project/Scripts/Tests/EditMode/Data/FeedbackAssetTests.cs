using Asteroids.Core.IConfigs;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Data.Tests
{
    /// <summary>
    /// The effects of the ship and of each enemy type are filled in on their own assets. These tests check that
    /// what the inspector writes is what the effects feature reads.
    /// </summary>
    [TestFixture]
    public sealed class FeedbackAssetTests
    {
        private TestAssets _assets;

        [SetUp]
        public void SetUp() => _assets = new TestAssets();

        [TearDown]
        public void TearDown() => _assets.DestroyAll();

        [Test]
        public void EnemyType_IsItsOwnEffectsEntry()
        {
            var type = _assets.EnemyType("Ufo");
            var hit = _assets.Effect("Sparks");
            var death = _assets.Effect("Boom");
            TestAssets.SetEffects(type, hit, true, death);

            IEnemyFeedbackConfig feedback = type;

            Assert.That(feedback.Type, Is.EqualTo(new EnemyTypeId("Ufo")));
            Assert.That(feedback.Hit, Is.SameAs(hit));
            Assert.That(feedback.PlayHitOnFatal, Is.True);
            Assert.That(feedback.Death, Is.SameAs(death));
        }

        [Test]
        public void ShipHealth_ExposesTheShipEffects()
        {
            var ship = ScriptableObject.CreateInstance<ShipHealthConfig>();
            try
            {
                var death = _assets.Effect("Boom");
                TestAssets.SetEffects(ship, null, false, death);

                Assert.That(ship.Effects.Hit, Is.Null);
                Assert.That(ship.Effects.Death, Is.SameAs(death));
            }
            finally
            {
                Object.DestroyImmediate(ship);
            }
        }

        [Test]
        public void DestroyedEffectAsset_ReadsAsNoEffect_NotAsADeadReference()
        {
            // Deleting an effect asset leaves a "missing" reference behind. VFX must see null there,
            // so it plays nothing instead of failing on a destroyed object.
            var type = _assets.EnemyType("Ufo");
            var death = _assets.Effect("Boom");
            TestAssets.SetEffects(type, death, false, death);

            Object.DestroyImmediate(death);
            IFeedbackConfig feedback = type;

            Assert.That(feedback.Hit, Is.Null);
            Assert.That(feedback.Death, Is.Null);
        }

        [Test]
        public void EffectDefinition_FindProblems_ChecksTheSharedSettings()
        {
            var effect = _assets.Effect("Boom");
            var serialized = new UnityEditor.SerializedObject(effect);
            serialized.FindProperty("maxActive").intValue = 0;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var problems = effect.FindProblems();

            Assert.That(problems, Has.Count.EqualTo(1));
            StringAssert.Contains("max active", problems[0]);
        }
    }
}

using Asteroids.Core.IConfigs;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Collisions.Tests
{
    public sealed class ColliderSetupRulesTests
    {
        [Test]
        public void HasNoLayer_LayerNone_ReturnsTrue() =>
            Assert.That(ColliderSetupRules.HasNoLayer(CollisionLayer.None), Is.True);

        [Test]
        public void HasNoLayer_AnyNonEmptyLayer_ReturnsFalse() =>
            Assert.That(ColliderSetupRules.HasNoLayer(CollisionLayer.Player), Is.False);

        [Test]
        public void OffsetReachesRadius_OffsetShorterThanRadius_ReturnsFalse() =>
            Assert.That(ColliderSetupRules.OffsetReachesRadius(Vector2.zero, 0.45f), Is.False);

        [Test]
        public void OffsetReachesRadius_OffsetEqualToRadius_ReturnsTrue() =>
            Assert.That(ColliderSetupRules.OffsetReachesRadius(new Vector2(0f, 0.5f), 0.5f), Is.True);

        [Test]
        public void OffsetReachesRadius_OffsetLongerThanRadius_ReturnsTrue() =>
            Assert.That(ColliderSetupRules.OffsetReachesRadius(new Vector2(0f, 0.5f), 0.1f), Is.True);
    }
}

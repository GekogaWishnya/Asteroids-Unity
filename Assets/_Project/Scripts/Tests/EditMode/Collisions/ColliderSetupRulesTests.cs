using Asteroids.Core.IConfigs;
using NUnit.Framework;

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
    }
}

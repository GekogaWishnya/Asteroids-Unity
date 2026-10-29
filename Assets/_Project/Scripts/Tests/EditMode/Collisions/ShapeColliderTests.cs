using System;
using Asteroids.Core.IConfigs;
using NUnit.Framework;

namespace Asteroids.Collisions.Tests
{
    [TestFixture]
    public sealed class ShapeColliderTests
    {
        [Test]
        public void Constructor_KeepsItsValues()
        {
            var owner = new object();
            var shape = new UnknownShape();

            var collider = new ShapeCollider(owner, CollisionLayer.Player, CollisionLayer.Enemy, shape);

            Assert.That(collider.Owner, Is.SameAs(owner));
            Assert.That(collider.Layer, Is.EqualTo(CollisionLayer.Player));
            Assert.That(collider.Mask, Is.EqualTo(CollisionLayer.Enemy));
            Assert.That(collider.Shape, Is.SameAs(shape));
        }

        [Test]
        public void Constructor_NullOwnerOrShape_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ShapeCollider(
                null, CollisionLayer.Player, CollisionLayer.All, new UnknownShape()));
            Assert.Throws<ArgumentNullException>(() => new ShapeCollider(
                new object(), CollisionLayer.Player, CollisionLayer.All, null));
        }

        [Test]
        public void Mask_CanBeChanged()
        {
            var collider = new ShapeCollider(
                new object(), CollisionLayer.Player, CollisionLayer.All, new UnknownShape());

            collider.Mask = CollisionLayer.Boundary;

            Assert.That(collider.Mask, Is.EqualTo(CollisionLayer.Boundary));
        }
    }
}

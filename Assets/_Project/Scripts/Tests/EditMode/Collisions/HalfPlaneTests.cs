using System;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Collisions.Tests
{
    [TestFixture]
    public sealed class HalfPlaneTests
    {
        private const float Tolerance = 1e-5f;

        [Test]
        public void MinX_PointInsideOnEdgeOutside_SignedDistanceIsPositiveZeroNegative()
        {
            var plane = HalfPlane.MinX(2f);

            Assert.That(plane.SignedDistance(new Vector2(5f, 7f)), Is.EqualTo(3f).Within(Tolerance));
            Assert.That(plane.SignedDistance(new Vector2(2f, -7f)), Is.EqualTo(0f).Within(Tolerance));
            Assert.That(plane.SignedDistance(new Vector2(1f, 0f)), Is.EqualTo(-1f).Within(Tolerance));
        }

        [Test]
        public void MaxX_PointInsideOnEdgeOutside_SignedDistanceIsPositiveZeroNegative()
        {
            var plane = HalfPlane.MaxX(2f);

            Assert.That(plane.SignedDistance(new Vector2(-1f, 7f)), Is.EqualTo(3f).Within(Tolerance));
            Assert.That(plane.SignedDistance(new Vector2(2f, -7f)), Is.EqualTo(0f).Within(Tolerance));
            Assert.That(plane.SignedDistance(new Vector2(3f, 0f)), Is.EqualTo(-1f).Within(Tolerance));
        }

        [Test]
        public void MinY_PointInsideOnEdgeOutside_SignedDistanceIsPositiveZeroNegative()
        {
            var plane = HalfPlane.MinY(-1f);

            Assert.That(plane.SignedDistance(new Vector2(7f, 4f)), Is.EqualTo(5f).Within(Tolerance));
            Assert.That(plane.SignedDistance(new Vector2(-7f, -1f)), Is.EqualTo(0f).Within(Tolerance));
            Assert.That(plane.SignedDistance(new Vector2(0f, -3f)), Is.EqualTo(-2f).Within(Tolerance));
        }

        [Test]
        public void MaxY_PointInsideOnEdgeOutside_SignedDistanceIsPositiveZeroNegative()
        {
            var plane = HalfPlane.MaxY(-1f);

            Assert.That(plane.SignedDistance(new Vector2(7f, -4f)), Is.EqualTo(3f).Within(Tolerance));
            Assert.That(plane.SignedDistance(new Vector2(-7f, -1f)), Is.EqualTo(0f).Within(Tolerance));
            Assert.That(plane.SignedDistance(new Vector2(0f, 2f)), Is.EqualTo(-3f).Within(Tolerance));
        }

        [Test]
        public void Constructor_NonUnitNormal_NormalisesNormalAndOffset()
        {
            var plane = new HalfPlane(new Vector2(0f, 2f), 4f);

            Assert.That(plane.InwardNormal.x, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(plane.InwardNormal.y, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(plane.Offset, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(plane.SignedDistance(new Vector2(0f, 2f)), Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void Constructor_ZeroNormal_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new HalfPlane(Vector2.zero, 1f));
        }

        [Test]
        public void Constructor_NormalShorterThanMinLength_ThrowsArgumentException()
        {
            var tiny = new Vector2(CollisionConstants.MinLength * 0.5f, 0f);

            Assert.Throws<ArgumentException>(() => new HalfPlane(tiny, 1f));
        }

        [Test]
        public void FromPoint_PointOnEdge_HasZeroSignedDistanceAtThatPoint()
        {
            var plane = HalfPlane.FromPoint(new Vector2(3f, 4f), Vector2.right);

            Assert.That(plane.SignedDistance(new Vector2(3f, -9f)), Is.EqualTo(0f).Within(Tolerance));
            Assert.That(plane.SignedDistance(new Vector2(4f, 0f)), Is.EqualTo(1f).Within(Tolerance));
        }
    }
}

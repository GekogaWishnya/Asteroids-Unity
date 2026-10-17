using Asteroids.Core.IConfigs;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.World.Tests
{
    [TestFixture]
    public sealed class WorldBoundsTests
    {
        private const float Tolerance = 1e-4f;

        private static WorldBounds Create(int width = 256, int height = 256, int pixelsPerUnit = 16) =>
            new(new TestWorldLayout(width, height, pixelsPerUnit));

        private static void AssertVector(Vector2 actual, float expectedX, float expectedY)
        {
            Assert.That(actual.x, Is.EqualTo(expectedX).Within(Tolerance), "x");
            Assert.That(actual.y, Is.EqualTo(expectedY).Within(Tolerance), "y");
        }

        [Test]
        public void Rect_SquareLayout_IsCentredOnOriginAndSizedInUnits()
        {
            var rect = Create(256, 256, 16).Rect;

            AssertVector(rect.min, -8f, -8f);
            AssertVector(rect.size, 16f, 16f);
        }

        [Test]
        public void Rect_NonSquareLayout_UsesEachAxisSeparately()
        {
            var rect = Create(320, 180, 16).Rect;

            AssertVector(rect.min, -10f, -5.625f);
            AssertVector(rect.size, 20f, 11.25f);
        }

        [Test]
        public void Rect_HigherPixelsPerUnit_ShrinksTheWorld()
        {
            var rect = Create(256, 256, 32).Rect;

            AssertVector(rect.min, -4f, -4f);
            AssertVector(rect.size, 8f, 8f);
        }

        [Test]
        public void Wrap_PointInside_IsReturnedUnchanged()
        {
            AssertVector(Create().Wrap(new Vector2(3f, -2f)), 3f, -2f);
        }

        [Test]
        public void Wrap_PointOnMinEdge_IsInsideAndUnchanged()
        {
            AssertVector(Create().Wrap(new Vector2(-8f, -8f)), -8f, -8f);
        }

        [Test]
        public void Wrap_PointOnMaxEdge_IsOutsideAndMovesToMinEdge()
        {
            AssertVector(Create().Wrap(new Vector2(8f, 8f)), -8f, -8f);
        }

        [Test]
        public void Wrap_PastMaxEdge_ReappearsOnTheOppositeSide()
        {
            AssertVector(Create().Wrap(new Vector2(9f, 0f)), -7f, 0f);
        }

        [Test]
        public void Wrap_BeforeMinEdge_ReappearsOnTheOppositeSide()
        {
            AssertVector(Create().Wrap(new Vector2(-9f, 0f)), 7f, 0f);
        }

        [Test]
        public void Wrap_AxesAreIndependent()
        {
            AssertVector(Create().Wrap(new Vector2(9f, 3f)), -7f, 3f);
        }

        [Test]
        public void Wrap_MultipleLapsAway_LandsInsideTheRect()
        {
            AssertVector(Create().Wrap(new Vector2(25f, -25f)), -7f, 7f);
        }

        [Test]
        public void Wrap_NonSquareRect_WrapsEachAxisByItsOwnSize()
        {
            var bounds = Create(320, 180, 16);

            AssertVector(bounds.Wrap(new Vector2(10.5f, 6f)), -9.5f, -5.25f);
        }

        [TestCase(9f, 0f)]
        [TestCase(-9f, 12.5f)]
        [TestCase(25f, -25f)]
        [TestCase(8f, 8f)]
        public void Wrap_AppliedTwice_GivesTheSameResultAsOnce(float x, float y)
        {
            var bounds = Create();

            var once = bounds.Wrap(new Vector2(x, y));
            var twice = bounds.Wrap(once);

            AssertVector(twice, once.x, once.y);
        }
    }
}

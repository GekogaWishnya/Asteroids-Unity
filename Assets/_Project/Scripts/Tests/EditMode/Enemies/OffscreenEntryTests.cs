using System;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Enemies.Tests
{
    /// <summary>
    /// <see cref="OffscreenEntry"/>: one depth outside every edge of the play area. The HUD is opaque and sits
    /// outside the area, so the area's edge is the edge of what the player sees.
    /// </summary>
    [TestFixture]
    public sealed class OffscreenEntryTests
    {
        private const float Tolerance = 1e-4f;

        // 2 units outside every edge of the 16 x 16 area.
        private static readonly OffscreenEntry Entry = new(2f);

        private static PerimeterPoint Bottom(float x) =>
            new(new Vector2(-8f, -8f), Vector2.right, Vector2.up, 16f, x + 8f);
        private static PerimeterPoint Top(float x) =>
            new(new Vector2(8f, 8f), Vector2.left, Vector2.down, 16f, 8f - x);
        private static PerimeterPoint Left(float y) =>
            new(new Vector2(-8f, 8f), Vector2.down, Vector2.right, 16f, 8f - y);
        private static PerimeterPoint Right(float y) =>
            new(new Vector2(8f, -8f), Vector2.up, Vector2.left, 16f, y + 8f);

        private static void AssertVector(Vector2 actual, float x, float y)
        {
            Assert.That(actual.x, Is.EqualTo(x).Within(Tolerance), "x");
            Assert.That(actual.y, Is.EqualTo(y).Within(Tolerance), "y");
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Constructor_NegativeOrNotFiniteDepth_Throws(float depth) =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new OffscreenEntry(depth));

        [Test]
        public void StartFor_StraightIn_BacksOffByTheDepthOnEveryEdge()
        {
            AssertVector(Entry.StartFor(Bottom(2f), Vector2.up), 2f, -10f);
            AssertVector(Entry.StartFor(Top(2f), Vector2.down), 2f, 10f);
            AssertVector(Entry.StartFor(Left(1f), Vector2.right), -10f, 1f);
            AssertVector(Entry.StartFor(Right(1f), Vector2.left), 10f, 1f);
        }

        [Test]
        public void StartFor_Slanted_BacksOffAlongTheDirectionSoThePathCrossesThePoint()
        {
            // Along (0.6, 0.8): into is 0.8, so the depth of 2 is 2.5 back along the path.
            var start = Entry.StartFor(Bottom(0f), new Vector2(3f, 4f));

            AssertVector(start, -1.5f, -10f);
        }

        [Test]
        public void StartFor_ShallowerThanTheMinimum_BacksOffStraightOut()
        {
            var shallow = new Vector2(1f, OffscreenEntry.MinEntryCosine * 0.5f);

            AssertVector(Entry.StartFor(Bottom(0f), shallow), 0f, -10f);
        }

        [Test]
        public void StartFor_NoDirection_BacksOffStraightOut()
        {
            AssertVector(Entry.StartFor(Left(0f), Vector2.zero), -10f, 0f);
            AssertVector(Entry.StartFor(Left(0f), new Vector2(float.NaN, 0f)), -10f, 0f);
        }

        [Test]
        public void StartFor_NoDistance_IsThePoint()
        {
            var onTheEdge = new OffscreenEntry(0f);

            AssertVector(onTheEdge.StartFor(Top(3f), new Vector2(1f, -1f)), 3f, 8f);
        }

        [Test]
        public void OutsideView_UsesTheMarginOnEveryEdge()
        {
            var entry = OffscreenEntry.OutsideView(new TestSpawnEntryConfig { SpawnMargin = 1f });

            AssertVector(entry.StartFor(Top(0f), Vector2.down), 0f, 9f);
            AssertVector(entry.StartFor(Bottom(0f), Vector2.up), 0f, -9f);
            AssertVector(entry.StartFor(Left(0f), Vector2.right), -9f, 0f);
            AssertVector(entry.StartFor(Right(0f), Vector2.left), 9f, 0f);
        }

        [Test]
        public void OutsideView_NullOrInvalid_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => OffscreenEntry.OutsideView(null));
            Assert.Throws<InvalidOperationException>(
                () => OffscreenEntry.OutsideView(new TestSpawnEntryConfig { SpawnMargin = -1f }));
        }
    }
}

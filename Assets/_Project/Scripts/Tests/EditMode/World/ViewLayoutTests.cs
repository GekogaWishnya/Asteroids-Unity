using System;
using Asteroids.Core.IConfigs;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.World.Tests
{
    [TestFixture]
    public sealed class ViewLayoutTests
    {
        private const float Tolerance = 1e-4f;

        private static ViewLayout Create(
            int top = 32, int bottom = 0, int width = 256, int height = 256, int ppu = 16) =>
            new(new TestWorldLayout(width, height, ppu), new TestHudLayout(top, bottom));

        private static void AssertVector(Vector2 actual, float expectedX, float expectedY)
        {
            Assert.That(actual.x, Is.EqualTo(expectedX).Within(Tolerance), "x");
            Assert.That(actual.y, Is.EqualTo(expectedY).Within(Tolerance), "y");
        }

        private static void AssertRect(Rect actual, float x, float y, float width, float height)
        {
            Assert.That(actual.x, Is.EqualTo(x).Within(Tolerance), "x");
            Assert.That(actual.y, Is.EqualTo(y).Within(Tolerance), "y");
            Assert.That(actual.width, Is.EqualTo(width).Within(Tolerance), "width");
            Assert.That(actual.height, Is.EqualTo(height).Within(Tolerance), "height");
        }

        [Test]
        public void Resolution_AddsBothBandHeightsToThePlayAreaHeight()
        {
            var layout = Create(top: 32, bottom: 16, width: 256, height: 256);

            Assert.That(layout.Resolution, Is.EqualTo(new Vector2Int(256, 304)));
        }

        [Test]
        public void Resolution_NoBands_IsThePlayArea()
        {
            Assert.That(Create(top: 0, bottom: 0, width: 320, height: 180).Resolution,
                Is.EqualTo(new Vector2Int(320, 180)));
        }

        [Test]
        public void PixelsPerUnit_ComesFromTheWorld() => Assert.That(Create(ppu: 8).PixelsPerUnit, Is.EqualTo(8));

        [Test]
        public void CameraOffset_TopBandOnly_MovesTheCameraUpByHalfTheBand() =>
            AssertVector(Create(top: 32, bottom: 0).CameraOffset, 0f, 1f);

        [Test]
        public void CameraOffset_BottomBandOnly_MovesTheCameraDownByHalfTheBand() =>
            AssertVector(Create(top: 0, bottom: 32).CameraOffset, 0f, -1f);

        [Test]
        public void CameraOffset_EqualBands_KeepsTheCameraOnTheOrigin() =>
            AssertVector(Create(top: 12, bottom: 12).CameraOffset, 0f, 0f);

        [Test]
        public void TopRect_SitsRightAboveThePlayArea() =>
            AssertRect(Create(top: 32, bottom: 16).GetHudRect(HudEdge.Top), -8f, 8f, 16f, 2f);

        [Test]
        public void BottomRect_SitsRightBelowThePlayArea() =>
            AssertRect(Create(top: 32, bottom: 16).GetHudRect(HudEdge.Bottom), -8f, -9f, 16f, 1f);

        [Test]
        public void HudRect_MissingBand_HasNoHeight() =>
            Assert.That(Create(top: 32, bottom: 0).GetHudRect(HudEdge.Bottom).height, Is.EqualTo(0f));

        [Test]
        public void HudRect_IsAsWideAsThePlayArea()
        {
            var layout = Create(top: 16, bottom: 16, width: 320, height: 180);

            Assert.That(layout.GetHudRect(HudEdge.Top).width, Is.EqualTo(20f).Within(Tolerance));
            Assert.That(layout.GetHudRect(HudEdge.Bottom).width, Is.EqualTo(20f).Within(Tolerance));
        }

        [TestCase(32, 0)]
        [TestCase(0, 32)]
        [TestCase(32, 16)]
        public void Bands_FillTheViewAroundThePlayArea(int top, int bottom)
        {
            var layout = Create(top: top, bottom: bottom, width: 256, height: 256, ppu: 16);
            var viewHeight = layout.Resolution.y / (float)layout.PixelsPerUnit;
            var viewBottom = layout.CameraOffset.y - viewHeight * 0.5f;
            var viewTop = layout.CameraOffset.y + viewHeight * 0.5f;

            Assert.That(layout.GetHudRect(HudEdge.Top).yMax, Is.EqualTo(viewTop).Within(Tolerance));
            Assert.That(layout.GetHudRect(HudEdge.Bottom).yMin, Is.EqualTo(viewBottom).Within(Tolerance));
            Assert.That(layout.GetHudRect(HudEdge.Top).yMin, Is.EqualTo(8f).Within(Tolerance));
            Assert.That(layout.GetHudRect(HudEdge.Bottom).yMax, Is.EqualTo(-8f).Within(Tolerance));
        }

        [Test]
        public void GetHudRect_UnknownEdge_Throws() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => Create().GetHudRect((HudEdge)42));

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new ViewLayout(null, new TestHudLayout(32, 0)));
            Assert.Throws<ArgumentNullException>(() => new ViewLayout(new TestWorldLayout(256, 256, 16), null));
        }

        [TestCase(-2, 0)]
        [TestCase(31, 0)]
        [TestCase(0, -2)]
        [TestCase(0, 15)]
        public void Constructor_BandHeightNegativeOrOdd_Throws(int top, int bottom)
        {
            Assert.Throws<ArgumentException>(() => Create(top: top, bottom: bottom));
        }

        [TestCase(0, 256, 16)]
        [TestCase(256, 0, 16)]
        [TestCase(256, 256, 0)]
        public void Constructor_NonPositivePlayAreaOrPixelsPerUnit_Throws(int width, int height, int ppu)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Create(width: width, height: height, ppu: ppu));
        }
    }
}

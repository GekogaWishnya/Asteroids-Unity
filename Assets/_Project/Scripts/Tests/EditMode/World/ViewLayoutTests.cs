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
            HudEdge edge = HudEdge.Top, int hudHeight = 32, int width = 256, int height = 256, int ppu = 16) =>
            new(new TestWorldLayout(width, height, ppu), new TestHudLayout(edge, hudHeight));

        private static void AssertVector(Vector2 actual, float expectedX, float expectedY)
        {
            Assert.That(actual.x, Is.EqualTo(expectedX).Within(Tolerance), "x");
            Assert.That(actual.y, Is.EqualTo(expectedY).Within(Tolerance), "y");
        }

        [Test]
        public void Resolution_AddsTheBandHeightToThePlayAreaHeight()
        {
            var layout = Create(hudHeight: 32, width: 256, height: 256);

            Assert.That(layout.Resolution, Is.EqualTo(new Vector2Int(256, 288)));
        }

        [Test]
        public void Resolution_NonSquarePlayArea_KeepsItsWidth()
        {
            var layout = Create(hudHeight: 20, width: 320, height: 180);

            Assert.That(layout.Resolution, Is.EqualTo(new Vector2Int(320, 200)));
        }

        [Test]
        public void PixelsPerUnit_IsPassedThrough()
        {
            Assert.That(Create(ppu: 32).PixelsPerUnit, Is.EqualTo(32));
        }

        [Test]
        public void CameraOffset_TopBand_MovesTheCameraUpByHalfTheBand()
        {
            AssertVector(Create(HudEdge.Top, 32).CameraOffset, 0f, 1f);
        }

        [Test]
        public void CameraOffset_BottomBand_MovesTheCameraDownByHalfTheBand()
        {
            AssertVector(Create(HudEdge.Bottom, 32).CameraOffset, 0f, -1f);
        }

        [Test]
        public void HudRect_TopBand_SitsAboveThePlayArea()
        {
            var rect = Create(HudEdge.Top, 32).HudRect;

            AssertVector(rect.min, -8f, 8f);
            AssertVector(rect.size, 16f, 2f);
        }

        [Test]
        public void HudRect_BottomBand_SitsBelowThePlayArea()
        {
            var rect = Create(HudEdge.Bottom, 32).HudRect;

            AssertVector(rect.min, -8f, -10f);
            AssertVector(rect.size, 16f, 2f);
        }

        [Test]
        public void HudRect_IsAsWideAsThePlayArea()
        {
            var rect = Create(hudHeight: 16, width: 320, height: 180).HudRect;

            AssertVector(rect.min, -10f, 5.625f);
            AssertVector(rect.size, 20f, 1f);
        }

        [TestCase(HudEdge.Top)]
        [TestCase(HudEdge.Bottom)]
        public void PlayArea_StaysCentredOnTheOriginInsideTheView(HudEdge edge)
        {
            var layout = Create(edge, 32, 256, 256, 16);
            var viewHeight = layout.Resolution.y / (float)layout.PixelsPerUnit;
            var viewBottom = layout.CameraOffset.y - viewHeight * 0.5f;
            var viewTop = layout.CameraOffset.y + viewHeight * 0.5f;

            var playAreaBottom = -8f;
            var playAreaTop = 8f;
            var hud = layout.HudRect;

            Assert.That(viewBottom, Is.LessThanOrEqualTo(Mathf.Min(playAreaBottom, hud.yMin) + Tolerance));
            Assert.That(viewTop, Is.GreaterThanOrEqualTo(Mathf.Max(playAreaTop, hud.yMax) - Tolerance));
            Assert.That(viewTop - viewBottom, Is.EqualTo(18f).Within(Tolerance));
        }

        [TestCase(HudEdge.Top)]
        [TestCase(HudEdge.Bottom)]
        public void HudRect_DoesNotOverlapThePlayArea(HudEdge edge)
        {
            var hud = Create(edge, 32, 256, 256, 16).HudRect;
            var playArea = new Rect(-8f, -8f, 16f, 16f);

            Assert.That(hud.Overlaps(playArea), Is.False);
        }

        [TestCase(HudEdge.Top)]
        [TestCase(HudEdge.Bottom)]
        public void HudRect_FillsTheViewNextToThePlayArea(HudEdge edge)
        {
            var layout = Create(edge, 32, 256, 256, 16);
            var viewHeight = layout.Resolution.y / (float)layout.PixelsPerUnit;
            var viewBottom = layout.CameraOffset.y - viewHeight * 0.5f;
            var viewTop = layout.CameraOffset.y + viewHeight * 0.5f;

            var hud = layout.HudRect;
            var touchesViewEdge = edge == HudEdge.Top
                ? Mathf.Abs(hud.yMax - viewTop) < Tolerance
                : Mathf.Abs(hud.yMin - viewBottom) < Tolerance;

            Assert.That(touchesViewEdge, Is.True);
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new ViewLayout(null, new TestHudLayout(HudEdge.Top, 32)));
            Assert.Throws<ArgumentNullException>(() => new ViewLayout(new TestWorldLayout(256, 256, 16), null));
        }

        [TestCase(0)]
        [TestCase(-2)]
        [TestCase(31)]
        public void Constructor_BandHeightNotPositiveAndEven_Throws(int heightPixels)
        {
            Assert.Throws<ArgumentException>(() => Create(hudHeight: heightPixels));
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

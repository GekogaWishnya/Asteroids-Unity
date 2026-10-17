using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.World.Tests
{
    internal sealed class TestWorldLayout : IWorldLayout
    {
        public TestWorldLayout(int width, int height, int pixelsPerUnit)
        {
            GameplayAreaResolution = new Vector2Int(width, height);
            PixelsPerUnit = pixelsPerUnit;
        }

        public Vector2Int GameplayAreaResolution { get; }
        public int PixelsPerUnit { get; }
    }

    internal sealed class TestHudLayout : IHudLayout
    {
        public TestHudLayout(HudEdge edge, int heightPixels)
        {
            Edge = edge;
            HeightPixels = heightPixels;
        }

        public HudEdge Edge { get; }
        public int HeightPixels { get; }
    }
}

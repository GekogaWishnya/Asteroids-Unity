using Asteroids.Core;
using UnityEngine;

namespace Asteroids.World
{
    public sealed class WorldBounds : IWorldBounds
    {
        public Rect Rect { get; }

        public WorldBounds(IWorldLayout layout)
        {
            var size = (Vector2)layout.ReferenceResolution / layout.PixelsPerUnit;
            Rect = new Rect(-size * 0.5f, size);
        }

        public Vector2 Wrap(Vector2 p, float radius = 0f)
        {
            var width = Rect.width + 2f * radius;
            var height = Rect.height + 2f * radius;
            return new Vector2(
                Mathf.Repeat(p.x - Rect.xMin + radius, width) - radius + Rect.xMin,
                Mathf.Repeat(p.y - Rect.yMin + radius, height) - radius + Rect.yMin);
        }
    }
}

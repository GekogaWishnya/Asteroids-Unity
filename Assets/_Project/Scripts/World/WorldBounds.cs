using Asteroids.Core.IConfigs;
using Asteroids.Physics;
using UnityEngine;

namespace Asteroids.World
{
    public sealed class WorldBounds : IWorldBounds
    {
        public Rect Rect { get; }

        public WorldBounds(IWorldLayout layout)
        {
            var size = (Vector2)layout.GameplayAreaResolution / layout.PixelsPerUnit;
            Rect = new Rect(-size * 0.5f, size);
        }

        public Vector2 Wrap(Vector2 position) => new(
            WrapAxis(position.x, Rect.xMin, Rect.width),
            WrapAxis(position.y, Rect.yMin, Rect.height));

        /// <summary>Points inside [min, min + size) are returned as they are.</summary>
        private static float WrapAxis(float value, float min, float size) =>
            value >= min && value < min + size
                ? value
                : Mathf.Repeat(value - min, size) + min;
    }
}

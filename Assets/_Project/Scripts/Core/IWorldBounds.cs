using UnityEngine;

namespace Asteroids.Core
{
    public interface IWorldBounds
    { Rect Rect { get; } Vector2 Wrap(Vector2 position, float radius = 0f); }
}

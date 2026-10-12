using UnityEngine;

namespace Asteroids.Core
{
    public interface IWorldConfig
    { Vector2Int Size { get; } Color BackgroundColor { get; } }
}

using UnityEngine;

namespace Asteroids.Core
{
    public interface IWorldLayout
    { Vector2Int ReferenceResolution { get; } int PixelsPerUnit { get; } }
}

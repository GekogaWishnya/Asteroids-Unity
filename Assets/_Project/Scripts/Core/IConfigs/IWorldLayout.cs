using UnityEngine;

namespace Asteroids.Core.IConfigs
{
    public interface IWorldLayout
    {
        Vector2Int ReferenceResolution { get; }
        int PixelsPerUnit { get; }
    }
}

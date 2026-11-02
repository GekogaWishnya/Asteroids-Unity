using UnityEngine;

namespace Asteroids.Core.IConfigs
{
    public interface IWorldLayout
    {
        Vector2Int GameplayAreaResolution { get; }
        int PixelsPerUnit { get; }
    }
}

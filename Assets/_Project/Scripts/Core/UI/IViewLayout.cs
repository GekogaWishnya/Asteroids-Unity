using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Core
{
    /// <summary>
    /// What the camera shows. Consists of the gameplay area and the HUD bands above and below it.
    /// The gameplay area stays centred on the world origin.
    /// </summary>
    public interface IViewLayout
    {
        /// <summary>Size of the whole view in pixels, the gameplay area and both HUD bands together.</summary>
        Vector2Int Resolution { get; }
        
        int PixelsPerUnit { get; }

        /// <summary>Center of the view is in world units.</summary>
        Vector2 CameraOffset { get; }

        /// <summary>The HUD band on <paramref name="edge"/> in world units.</summary>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="edge"/> is not a known edge.</exception>
        Rect GetHudRect(HudEdge edge);
    }
}

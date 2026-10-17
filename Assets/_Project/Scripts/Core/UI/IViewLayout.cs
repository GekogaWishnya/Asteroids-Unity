using UnityEngine;

namespace Asteroids.Core
{
    /// <summary>
    /// What the camera shows. Consists of the gameplay area and the HUD band.
    /// The gameplay area stays centred on the world origin.
    /// </summary>
    public interface IViewLayout
    {
        /// <summary>Size of the whole view in pixels, the gameplay area and the HUD band together.</summary>
        Vector2Int Resolution { get; }
        
        int PixelsPerUnit { get; }

        /// <summary>Center of the view is in world units.</summary>
        Vector2 CameraOffset { get; }

        /// <summary>The HUD band in world units.</summary>
        Rect HudRect { get; }
    }
}
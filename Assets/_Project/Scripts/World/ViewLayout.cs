using System;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.World
{
    /// <summary>
    /// <para>
    /// Puts the HUD band next to the gameplay area and works out what the camera has to show.
    /// The gameplay area keeps its size and stays centred on the origin, so wrapping and physics are unaffected.
    /// The view grows by the height of the band and the camera moves by half of it.
    /// </para>
    /// <para>
    /// The band is as wide as the play area, whatever the world's layout is.
    /// </para>
    /// </summary>
    public sealed class ViewLayout : IViewLayout
    {
        /// <exception cref="ArgumentNullException">An argument is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Pixels per unit or a side of the play area is not positive.
        /// </exception>
        /// <exception cref="ArgumentException">The band height is not positive and even.</exception>
        public ViewLayout(IWorldLayout world, IHudLayout hud)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (hud == null) throw new ArgumentNullException(nameof(hud));

            var gameplayArea = world.GameplayAreaResolution;
            if (gameplayArea.x <= 0 || gameplayArea.y <= 0)
                throw new ArgumentOutOfRangeException(nameof(world), "The gameplay area must have a positive size.");
            if (world.PixelsPerUnit <= 0)
                throw new ArgumentOutOfRangeException(nameof(world), "Pixels per unit must be positive.");
            if (hud.HeightPixels <= 0 || (hud.HeightPixels & 1) != 0)
                throw new ArgumentException("The HUD band height must be positive and even.", nameof(hud));

            PixelsPerUnit = world.PixelsPerUnit;
            Resolution = new Vector2Int(gameplayArea.x, gameplayArea.y + hud.HeightPixels);

            var playAreaSize = (Vector2)gameplayArea / PixelsPerUnit;
            var bandHeight = (float)hud.HeightPixels / PixelsPerUnit;
            var direction = hud.Edge == HudEdge.Top ? 1f : -1f;

            CameraOffset = new Vector2(0f, direction * bandHeight * 0.5f);

            var bandCentreY = direction * (playAreaSize.y + bandHeight) * 0.5f;
            HudRect = new Rect(
                -playAreaSize.x * 0.5f,
                bandCentreY - bandHeight * 0.5f,
                playAreaSize.x,
                bandHeight);
        }

        public Vector2Int Resolution { get; }
        public int PixelsPerUnit { get; }
        public Vector2 CameraOffset { get; }
        public Rect HudRect { get; }
    }
}

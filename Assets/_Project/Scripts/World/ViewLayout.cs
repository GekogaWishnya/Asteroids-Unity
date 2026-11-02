using System;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.World
{
    /// <summary>
    /// Puts the HUD bands next to the gameplay area and works out what the camera has to show.
    /// The gameplay area keeps its size and stays centred on the origin, so wrapping and physics are unaffected.
    /// The view grows by the height of the band and the camera moves by half of it.
    /// </summary>
    public sealed class ViewLayout : IViewLayout
    {
        private readonly Rect _top;
        private readonly Rect _bottom;

        /// <exception cref="ArgumentNullException">An argument is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Pixels per unit or a side of the play area is not positive.
        /// </exception>
        /// <exception cref="ArgumentException">A band height is negative or odd.</exception>
        public ViewLayout(IWorldLayout world, IHudLayout hud)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (hud == null) throw new ArgumentNullException(nameof(hud));

            var gameplayArea = world.GameplayAreaResolution;
            if (gameplayArea.x <= 0 || gameplayArea.y <= 0)
                throw new ArgumentOutOfRangeException(nameof(world), "The gameplay area must have a positive size.");
            if (world.PixelsPerUnit <= 0)
                throw new ArgumentOutOfRangeException(nameof(world), "Pixels per unit must be positive.");
            if (!IsValidBand(hud.TopHeightPixels))
                throw new ArgumentException("The top HUD band height must be even and not negative.", nameof(hud));
            if (!IsValidBand(hud.BottomHeightPixels))
                throw new ArgumentException("The bottom HUD band height must be even and not negative.", nameof(hud));

            PixelsPerUnit = world.PixelsPerUnit;
            Resolution = new Vector2Int(
                gameplayArea.x, gameplayArea.y + hud.TopHeightPixels + hud.BottomHeightPixels);

            var playAreaSize = (Vector2)gameplayArea / PixelsPerUnit;
            var topHeight = (float)hud.TopHeightPixels / PixelsPerUnit;
            var bottomHeight = (float)hud.BottomHeightPixels / PixelsPerUnit;
            var halfPlayHeight = playAreaSize.y * 0.5f;
            var left = -playAreaSize.x * 0.5f;

            CameraOffset = new Vector2(0f, (topHeight - bottomHeight) * 0.5f);
            _top = new Rect(left, halfPlayHeight, playAreaSize.x, topHeight);
            _bottom = new Rect(left, -halfPlayHeight - bottomHeight, playAreaSize.x, bottomHeight);
        }

        public Vector2Int Resolution { get; }
        public int PixelsPerUnit { get; }
        public Vector2 CameraOffset { get; }

        public Rect GetHudRect(HudEdge edge) => edge switch
        {
            HudEdge.Top => _top,
            HudEdge.Bottom => _bottom,
            _ => throw new ArgumentOutOfRangeException(nameof(edge), edge, "Unknown HUD edge.")
        };

        private static bool IsValidBand(int heightPixels) => heightPixels >= 0 && (heightPixels & 1) == 0;
    }
}

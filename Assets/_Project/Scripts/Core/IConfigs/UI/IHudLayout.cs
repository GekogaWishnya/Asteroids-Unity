namespace Asteroids.Core.IConfigs
{
    /// <summary>
    /// Bands reserved for the HUD. They lie outside the gameplay area, so they never affect the size of the world.
    /// </summary>
    public interface IHudLayout
    {
        /// <summary>Height of the band above the gameplay area in pixels.</summary>
        int TopHeightPixels { get; }

        /// <summary>Height of the band below the gameplay area in pixels.</summary>
        int BottomHeightPixels { get; }
    }
}
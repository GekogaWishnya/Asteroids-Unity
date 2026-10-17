namespace Asteroids.Core.IConfigs
{
    /// <summary>
    /// The band reserved for the HUD. It lies outside the gameplay area, so it never changes the size of the world.
    /// </summary>
    public interface IHudLayout
    {
        HudEdge Edge { get; }

        /// <summary>Height of the band in pixels. Positive and even, so the play area stays on the pixel grid.</summary>
        int HeightPixels { get; }
    }
}

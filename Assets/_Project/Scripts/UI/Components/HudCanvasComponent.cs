using Asteroids.Core;
using UnityEngine;
using Zenject;

namespace Asteroids.UI.Components
{
    /// <summary>
    /// <para>
    /// Puts the HUD canvas on the HUD band of the view.
    /// </para>
    /// <para>
    /// One canvas unit is one pixel of the gameplay area. The canvas is scaled by <c>1 / PixelsPerUnit</c>,
    /// sized to the band and centred on it, so children can be laid out in pixels with ordinary anchors.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [DisallowMultipleComponent]
    [AddComponentMenu("Asteroids/UI/Hud Canvas")]
    public sealed class HudCanvasComponent : MonoBehaviour
    {
        [Inject]
        private void Construct(IViewLayout layout)
        {
            GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;

            var band = layout.HudRect;
            var pixelsPerUnit = layout.PixelsPerUnit;

            var rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one / pixelsPerUnit;
            rect.sizeDelta = band.size * pixelsPerUnit;
            rect.position = new Vector3(band.center.x, band.center.y, rect.position.z);
        }
    }
}
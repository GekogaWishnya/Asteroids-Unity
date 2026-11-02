using Asteroids.Core.IConfigs;
using UnityEngine;
using UnityEngine.Serialization;

namespace Asteroids.Data
{
    [CreateAssetMenu(fileName = "Hud", menuName = "Asteroids/Configs/Hud")]
    public sealed class HudConfig : ScriptableObject, IHudLayout
    {
        [Tooltip("Height of the HUD band above the gameplay area in pixels.")]
        [FormerlySerializedAs("heightPixels")]
        [SerializeField, Delayed] private int topHeightPixels = 12;
        [Tooltip("Height of the HUD band below the gameplay area in pixels.")]
        [SerializeField, Delayed] private int bottomHeightPixels = 12;

        public int TopHeightPixels => topHeightPixels;
        public int BottomHeightPixels => bottomHeightPixels;

        private void OnValidate()
        {
            topHeightPixels = Mathf.Max(0, topHeightPixels & ~1);
            bottomHeightPixels = Mathf.Max(0, bottomHeightPixels & ~1);
        }
    }
}

using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Data
{
    [CreateAssetMenu(fileName = "Hud", menuName = "Asteroids/Configs/Hud")]
    public sealed class HudConfig : ScriptableObject, IHudLayout
    {
        [Tooltip("Side of the gameplay area the HUD band is attached to")]
        [SerializeField] private HudEdge edge = HudEdge.Top;
        [Tooltip("Height of the HUD band in pixels")]
        [SerializeField, Delayed] private int heightPixels = 32;

        public HudEdge Edge => edge;
        public int HeightPixels => heightPixels;

        private void OnValidate()
        {
            heightPixels = Mathf.Max(2, heightPixels & ~1);
        }
    }
}
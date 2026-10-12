using Asteroids.Core;
using UnityEngine;

namespace Asteroids.Data
{
    [CreateAssetMenu(fileName = "WorldConfig", menuName = "Asteroids/Configs/World")]
    public sealed class WorldConfig : ScriptableObject, IWorldLayout, IWorldAppearance
    {
        [Header("Layout")]
        [Tooltip("Gameplay space width in pixels")]
        [SerializeField, Delayed] private int referenceWidth  = 256;
        [Tooltip("Gameplay space height in pixels")]
        [SerializeField, Delayed] private int referenceHeight = 256;
        [Tooltip("PPU of sprites")]
        [SerializeField, Min(1)] private int pixelsPerUnit = 16;
        
        [Header("Appearance")]
        [Tooltip("Background color of the gameplay space")]
        [SerializeField, ColorUsage(false)] private Color backgroundColor = new(0.0854174f, 0.01811143f, 0.1037736f);
        
        public Vector2Int ReferenceResolution => new(referenceWidth, referenceHeight);
        public int PixelsPerUnit => pixelsPerUnit;
        public Color BackgroundColor => backgroundColor;

        private void OnValidate()
        {
            referenceWidth = Mathf.Max(2, referenceWidth & ~1);
            referenceHeight = Mathf.Max(2, referenceHeight & ~1);
        }
    }
}

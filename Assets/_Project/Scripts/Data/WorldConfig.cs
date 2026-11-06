using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Data
{
    [CreateAssetMenu(fileName = "WorldConfig", menuName = "Asteroids/Configs/World")]
    public sealed class WorldConfig : ScriptableObject, IWorldLayout, IWorldAppearance, ISpawnEntryConfig
    {
        [Header("Layout")]
        [Tooltip("Gameplay space width in pixels")]
        [SerializeField, Delayed] private int referenceWidth = 256;
        [Tooltip("Gameplay space height in pixels")]
        [SerializeField, Delayed] private int referenceHeight = 256;
        [Tooltip("PPU of sprites")]
        [SerializeField, Min(1)] private int pixelsPerUnit = 16;

        [Header("Spawn entry")]
        [Tooltip("World units beyond the visible edges, where enemies start before flying in")]
        [SerializeField, Min(0f)] private float spawnMargin = 1f;
        [Tooltip("Seconds an entering enemy may stay outside the gameplay space before it is moved onto its edge")]
        [SerializeField, Min(0.1f)] private float maxEnteringSeconds = 10f;

        [Header("Appearance")]
        [Tooltip("Background color of the gameplay space")]
        [SerializeField, ColorUsage(false)] private Color backgroundColor = new(0.08f, 0.02f, 0.1f);

        public Vector2Int GameplayAreaResolution => new(referenceWidth, referenceHeight);
        public int PixelsPerUnit => pixelsPerUnit;
        public Color BackgroundColor => backgroundColor;
        public float SpawnMargin => spawnMargin;
        public float MaxEnteringSeconds => maxEnteringSeconds;

        private void OnValidate()
        {
            referenceWidth = Mathf.Max(2, referenceWidth & ~1);
            referenceHeight = Mathf.Max(2, referenceHeight & ~1);
        }
    }
}
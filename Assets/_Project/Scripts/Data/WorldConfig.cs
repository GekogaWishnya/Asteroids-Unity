using Asteroids.Core;
using UnityEngine;

namespace Asteroids.Data
{
    [CreateAssetMenu(fileName = "WorldConfig", menuName = "Asteroids/Configs/World")]
    public sealed class WorldConfig : ScriptableObject, IWorldConfig
    {
        [Tooltip("Gameplay space size in pixels")]
        [SerializeField, Min(0)] private Vector2Int size = new(256, 256);

        [Tooltip("Background color of the gameplay space")]
        [SerializeField] private Color backgroundColor = new(0.0854174f, 0.01811143f, 0.1037736f, 1f);
        
        public Vector2Int Size => size;
        public Color BackgroundColor => backgroundColor;
    }
}

using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Data
{
    [CreateAssetMenu(fileName = "RunEnd", menuName = "Asteroids/Configs/Run End")]
    public sealed class RunEndConfig : ScriptableObject, IRunEndConfig
    {
        [Tooltip("Seconds the world runs after the ship is destroyed before the run ends")]
        [SerializeField, Min(0f)] private float defeatDelay = 1.5f;

        [Tooltip("Seconds the world runs after the last wave is cleared before the run ends")]
        [SerializeField, Min(0f)] private float victoryDelay = 1f;

        public float DefeatDelay => defeatDelay;
        public float VictoryDelay => victoryDelay;
    }
}
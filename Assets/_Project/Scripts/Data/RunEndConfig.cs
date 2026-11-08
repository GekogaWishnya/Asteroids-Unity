using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Data
{
    [CreateAssetMenu(fileName = "RunEnd", menuName = "Asteroids/Configs/Run End")]
    public sealed class RunEndConfig : ScriptableObject, IRunEndConfig, IRunEndScreenConfig
    {
        [Tooltip("Seconds the world runs after the ship is destroyed before the run ends")]
        [SerializeField, Min(0f)] private float defeatDelay = 1.5f;
        [Tooltip("Seconds the world runs after the last wave is cleared before the run ends")]
        [SerializeField, Min(0f)] private float victoryDelay = 1f;

        [Header("Screen")]
        [Tooltip("Title after the ship was destroyed")]
        [SerializeField] private string defeatTitle = "GAME OVER";
        [Tooltip("Title after every wave was cleared")]
        [SerializeField] private string victoryTitle = "STAGE COMPLETE";
        [Tooltip("{0} is the final score. \"D6\" pads it with zeros to six digits")]
        [SerializeField] private string scoreFormat = "SCORE {0:D6}";
        [Tooltip("{0} is the number of waves cleared and {1} the number of waves in the level")]
        [SerializeField] private string wavesFormat = "WAVES {0}/{1}";

        public float DefeatDelay => defeatDelay;
        public float VictoryDelay => victoryDelay;

        public string DefeatTitle => defeatTitle;
        public string VictoryTitle => victoryTitle;
        public string ScoreFormat => scoreFormat;
        public string WavesFormat => wavesFormat;
    }
}
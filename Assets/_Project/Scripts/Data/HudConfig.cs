using System.Collections.Generic;
using Asteroids.Core.IConfigs;
using UnityEngine;
using UnityEngine.Serialization;

namespace Asteroids.Data
{
    [CreateAssetMenu(fileName = "Hud", menuName = "Asteroids/Configs/Hud")]
    public sealed class HudConfig : ScriptableObject, IHudLayout, IScoreHudConfig, IHealthHudConfig, ILaserHudConfig,
        ITelemetryHudConfig
    {
        [Tooltip("Height of the HUD band above the gameplay area in pixels.")]
        [SerializeField, Delayed] private int topHeightPixels = 12;
        [Tooltip("Height of the HUD band below the gameplay area in pixels.")]
        [SerializeField, Delayed] private int bottomHeightPixels = 12;

        [Header("Score")]
        [Tooltip("{0} is the score. \"D6\" pads it with zeros to six digits.")]
        [SerializeField] private string scoreFormat = "SCORE {0:D6}";

        [Header("Health")]
        [Tooltip("{0} is the current health and {1} the maximum.")]
        [SerializeField] private string healthFormat = "HP {0}/{1}";

        [Header("Laser")]
        [Tooltip("{0} is the energy in whole percent.")]
        [SerializeField] private string energyFormat = "LASER {0}%";
        [Tooltip("{0} is the seconds until full, rounded up to a tenth.")]
        [SerializeField] private string refillFormat = "FULL IN {0:0.0}s";
        [Tooltip("Text shown when the energy is fully refilled.")]
        [SerializeField] private string fullText = "FULL";

        [Header("Telemetry")]
        [Tooltip("{0} is X and {1} is Y, rounded to a tenth.")]
        [SerializeField] private string positionFormat = "X {0:0.0}  Y {1:0.0}";
        [Tooltip("{0} is whole degrees from 0 to 359.")]
        [SerializeField] private string angleFormat = "ANG {0}°";
        [Tooltip("{0} is units per second, rounded to a tenth.")]
        [SerializeField] private string speedFormat = "SPD {0:0.0}";
        [SerializeField] private string noShipPositionText = "X --  Y --";
        [SerializeField] private string noShipAngleText = "ANG --";
        [SerializeField] private string noShipSpeedText = "SPD --";

        public int TopHeightPixels => topHeightPixels;
        public int BottomHeightPixels => bottomHeightPixels;

        public string ScoreFormat => scoreFormat;
        public string HealthFormat => healthFormat;
        public string EnergyFormat => energyFormat;
        public string RefillFormat => refillFormat;
        public string FullText => fullText;

        public string PositionFormat => positionFormat;
        public string AngleFormat => angleFormat;
        public string SpeedFormat => speedFormat;
        public string NoShipPositionText => noShipPositionText;
        public string NoShipAngleText => noShipAngleText;
        public string NoShipSpeedText => noShipSpeedText;

        private void OnValidate()
        {
            topHeightPixels = Mathf.Max(0, topHeightPixels & ~1);
            bottomHeightPixels = Mathf.Max(0, bottomHeightPixels & ~1);

            Warn(ScoreHudConfigValidator.Validate(this));
            Warn(HealthHudConfigValidator.Validate(this));
            Warn(LaserHudConfigValidator.Validate(this));
            Warn(TelemetryHudConfigValidator.Validate(this));
        }

        private void Warn(IReadOnlyList<string> problems)
        {
            foreach (var problem in problems)
                Debug.LogWarning($"{name}: {problem}", this);
        }
    }
}
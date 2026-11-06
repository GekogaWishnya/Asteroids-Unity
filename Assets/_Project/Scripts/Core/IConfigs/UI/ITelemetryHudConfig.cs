namespace Asteroids.Core.IConfigs
{
    /// <summary>
    /// Formatting (<see cref="string.Format(string, object)"/>) of the ship's telemetry to show on HUD.
    /// </summary>
    public interface ITelemetryHudConfig
    {
        /// <summary>
        /// {0} is X and {1} is Y, in world units, already rounded to a tenth. For example "X {0:0.0}  Y {1:0.0}".
        /// </summary>
        string PositionFormat { get; }

        /// <summary>{0} is whole degrees in [0, 360). For example "ANG {0}°".</summary>
        string AngleFormat { get; }

        /// <summary>{0} is units per second, already rounded to a tenth. For example "SPD {0:0.0}".</summary>
        string SpeedFormat { get; }

        string NoShipPositionText { get; }
        string NoShipAngleText { get; }
        string NoShipSpeedText { get; }
    }
}
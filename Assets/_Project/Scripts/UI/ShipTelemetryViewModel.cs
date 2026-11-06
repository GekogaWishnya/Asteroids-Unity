using System;
using System.Globalization;
using Asteroids.Common;
using Asteroids.Core;
using Asteroids.Core.IConfigs;

namespace Asteroids.UI
{
    /// <summary>What the ship's telemetry looks like on the HUD. Without a ship every text shows dashes.</summary>
    public sealed class ShipTelemetryViewModel : IRefreshable
    {
        private readonly IShipTelemetry _telemetry;
        private readonly ITelemetryHudConfig _config;
        private readonly ObservableValue<string> _positionText;
        private readonly ObservableValue<string> _angleText;
        private readonly ObservableValue<string> _speedText;

        private int? _shownX;
        private int? _shownY;
        private int? _shownDegrees;
        private int? _shownSpeed;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        /// <exception cref="InvalidOperationException">The config is invalid.</exception>
        public ShipTelemetryViewModel(IShipTelemetry telemetry, ITelemetryHudConfig config)
        {
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            ConfigValidation.ThrowIfInvalid("The telemetry HUD config", TelemetryHudConfigValidator.Validate(config));

            Read(out _shownX, out _shownY, out _shownDegrees, out _shownSpeed);
            _positionText = new ObservableValue<string>(FormatPosition(_shownX, _shownY));
            _angleText = new ObservableValue<string>(FormatAngle(_shownDegrees));
            _speedText = new ObservableValue<string>(FormatSpeed(_shownSpeed));
        }

        public IObservableValue<string> PositionText => _positionText;
        public IObservableValue<string> AngleText => _angleText;
        public IObservableValue<string> SpeedText => _speedText;

        public void Refresh()
        {
            Read(out var x, out var y, out var degrees, out var speed);

            if (x != _shownX || y != _shownY)
            {
                _shownX = x;
                _shownY = y;
                _positionText.Value = FormatPosition(x, y);
            }

            if (degrees != _shownDegrees)
            {
                _shownDegrees = degrees;
                _angleText.Value = FormatAngle(degrees);
            }

            if (speed != _shownSpeed)
            {
                _shownSpeed = speed;
                _speedText.Value = FormatSpeed(speed);
            }
        }

        private void Read(out int? x, out int? y, out int? degrees, out int? speed)
        {
            if (!_telemetry.HasShip)
            {
                x = y = degrees = speed = null;
                return;
            }

            var position = _telemetry.Position;
            x = ToTenths(position.x);
            y = ToTenths(position.y);
            degrees = ToDegrees(_telemetry.Angle);
            speed = ToTenths(Math.Max(0f, _telemetry.Speed));
        }

        private static int ToTenths(float value) =>
            (int)Math.Round(value * 10.0, MidpointRounding.AwayFromZero);

        private static int ToDegrees(float angle)
        {
            var degrees = (int)Math.Round(angle, MidpointRounding.AwayFromZero) % 360;
            return degrees < 0 ? degrees + 360 : degrees;
        }

        private string FormatPosition(int? x, int? y) =>
            x.HasValue && y.HasValue
                ? string.Format(CultureInfo.InvariantCulture, _config.PositionFormat, x.Value / 10.0, y.Value / 10.0)
                : _config.NoShipPositionText;

        private string FormatAngle(int? degrees) =>
            degrees.HasValue
                ? string.Format(CultureInfo.InvariantCulture, _config.AngleFormat, degrees.Value)
                : _config.NoShipAngleText;

        private string FormatSpeed(int? speed) =>
            speed.HasValue
                ? string.Format(CultureInfo.InvariantCulture, _config.SpeedFormat, speed.Value / 10.0)
                : _config.NoShipSpeedText;
    }
}
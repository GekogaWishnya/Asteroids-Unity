using System;
using System.Globalization;
using Asteroids.Common;
using Asteroids.Core;

namespace Asteroids.UI
{
    /// <summary>What the ship's telemetry looks like on the HUD. Without a ship every text shows dashes.</summary>
    public sealed class ShipTelemetryViewModel : IRefreshable
    {
        private const string PositionFormat = "X {0:0.0}  Y {1:0.0}";
        private const string AngleFormat = "ANG {0}°";
        private const string SpeedFormat = "SPD {0:0.0}";
        private const string NoPositionText = "X --  Y --";
        private const string NoAngleText = "ANG --";
        private const string NoSpeedText = "SPD --";

        private readonly IShipTelemetry _telemetry;
        private readonly ObservableValue<string> _positionText;
        private readonly ObservableValue<string> _angleText;
        private readonly ObservableValue<string> _speedText;

        private int? _shownX;
        private int? _shownY;
        private int? _shownDegrees;
        private int? _shownSpeed;

        /// <exception cref="ArgumentNullException"><paramref name="telemetry"/> is null.</exception>
        public ShipTelemetryViewModel(IShipTelemetry telemetry)
        {
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));

            Read(out _shownX, out _shownY, out _shownDegrees, out _shownSpeed);
            _positionText = new ObservableValue<string>(FormatPosition(_shownX, _shownY));
            _angleText = new ObservableValue<string>(FormatAngle(_shownDegrees));
            _speedText = new ObservableValue<string>(FormatSpeed(_shownSpeed));
        }

        /// <summary>For example "X 12.3  Y -4.0" or "X --  Y --".</summary>
        public IObservableValue<string> PositionText => _positionText;

        /// <summary>For example "ANG 90°" or "ANG --".</summary>
        public IObservableValue<string> AngleText => _angleText;

        /// <summary>For example "SPD 3.2" or "SPD --".</summary>
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

        private static string FormatPosition(int? x, int? y) =>
            x.HasValue && y.HasValue
                ? string.Format(CultureInfo.InvariantCulture, PositionFormat, x.Value / 10.0, y.Value / 10.0)
                : NoPositionText;

        private static string FormatAngle(int? degrees) =>
            degrees.HasValue ? string.Format(CultureInfo.InvariantCulture, AngleFormat, degrees.Value) : NoAngleText;

        private static string FormatSpeed(int? speed) =>
            speed.HasValue
                ? string.Format(CultureInfo.InvariantCulture, SpeedFormat, speed.Value / 10.0)
                : NoSpeedText;
    }
}
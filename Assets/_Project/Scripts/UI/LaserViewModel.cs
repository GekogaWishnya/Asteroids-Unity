using System;
using System.Globalization;
using Asteroids.Common;
using Asteroids.Core;
using Asteroids.Core.IConfigs;

namespace Asteroids.UI
{
    public sealed class LaserViewModel : IDisposable
    {
        private readonly ILaserHudConfig _config;
        private readonly ObservableValue<string> _energyText;
        private readonly ObservableValue<string> _refillText;
        private readonly IDisposable _energySubscription;
        private readonly IDisposable _refillSubscription;

        private int _shownPercent;
        private int _shownTenths;

        /// <exception cref="ArgumentNullException"><paramref name="laser"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> is null.</exception>
        /// <exception cref="InvalidOperationException">The config is invalid.</exception>
        public LaserViewModel(ILaserStatus laser, ILaserHudConfig config)
        {
            if (laser == null) throw new ArgumentNullException(nameof(laser));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            ConfigValidation.ThrowIfInvalid("The laser HUD config", LaserHudConfigValidator.Validate(config));

            _shownPercent = ToPercent(laser.Energy.Value);
            _shownTenths = ToTenths(laser.SecondsToFull.Value);
            _energyText = new ObservableValue<string>(FormatEnergy(_shownPercent));
            _refillText = new ObservableValue<string>(FormatRefill(_shownTenths));

            _energySubscription = laser.Energy.Subscribe(OnEnergyChanged);
            _refillSubscription = laser.SecondsToFull.Subscribe(OnSecondsToFullChanged);
        }

        public IObservableValue<string> EnergyText => _energyText;
        public IObservableValue<string> RefillText => _refillText;

        public void Dispose()
        {
            _energySubscription.Dispose();
            _refillSubscription.Dispose();
        }

        private void OnEnergyChanged(float fraction)
        {
            var percent = ToPercent(fraction);
            if (percent == _shownPercent) return;

            _shownPercent = percent;
            _energyText.Value = FormatEnergy(percent);
        }

        private void OnSecondsToFullChanged(float seconds)
        {
            var tenths = ToTenths(seconds);
            if (tenths == _shownTenths) return;

            _shownTenths = tenths;
            _refillText.Value = FormatRefill(tenths);
        }

        private static int ToPercent(float fraction) =>
            (int)Math.Floor(Math.Clamp(fraction, 0f, 1f) * 100.0);

        private static int ToTenths(float seconds) =>
            seconds > 0f ? (int)Math.Max(1.0, Math.Ceiling(Math.Min(seconds, 1e6f) * 10.0)) : 0;

        private string FormatEnergy(int percent) =>
            string.Format(CultureInfo.InvariantCulture, _config.EnergyFormat, percent);

        private string FormatRefill(int tenths) =>
            tenths == 0
                ? _config.FullText
                : string.Format(CultureInfo.InvariantCulture, _config.RefillFormat, tenths / 10.0);
    }
}
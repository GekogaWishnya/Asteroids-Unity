using System;
using System.Globalization;
using Asteroids.Common;
using Asteroids.Core;

namespace Asteroids.UI
{
    public sealed class LaserViewModel : IDisposable
    {
        private const string EnergyFormat = "LASER {0}%";
        private const string RefillFormat = "FULL IN {0:0.0}s";
        private const string FullText = "FULL";

        private readonly ObservableValue<string> _energyText;
        private readonly ObservableValue<string> _refillText;
        private readonly IDisposable _energySubscription;
        private readonly IDisposable _refillSubscription;
        
        private int _shownPercent;
        private int _shownTenths;

        /// <exception cref="ArgumentNullException"><paramref name="laser"/> is null.</exception>
        public LaserViewModel(ILaserStatus laser)
        {
            if (laser == null) throw new ArgumentNullException(nameof(laser));

            _shownPercent = ToPercent(laser.Energy.Value);
            _shownTenths = ToTenths(laser.SecondsToFull.Value);
            _energyText = new ObservableValue<string>(FormatEnergy(_shownPercent));
            _refillText = new ObservableValue<string>(FormatRefill(_shownTenths));

            _energySubscription = laser.Energy.Subscribe(OnEnergyChanged);
            _refillSubscription = laser.SecondsToFull.Subscribe(OnSecondsToFullChanged);
        }

        /// <summary>For example "LASER 73%".</summary>
        public IObservableValue<string> EnergyText => _energyText;

        /// <summary>For example "FULL IN 4.2s", or "FULL".</summary>
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

        private static string FormatEnergy(int percent) =>
            string.Format(CultureInfo.InvariantCulture, EnergyFormat, percent);

        private static string FormatRefill(int tenths) =>
            tenths == 0 ? FullText : string.Format(CultureInfo.InvariantCulture, RefillFormat, tenths / 10.0);
    }
}

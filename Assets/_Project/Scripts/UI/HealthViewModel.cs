using System;
using System.Globalization;
using Asteroids.Common;
using Asteroids.Core;

namespace Asteroids.UI
{
    /// <summary>
    /// What the player's health looks like on the HUD. <see cref="Text"/> is always ready to show,
    /// Follows the health until disposed.
    /// </summary>
    public sealed class HealthViewModel : IDisposable
    {
        private const string TextFormat = "HP {0}/{1}";

        private readonly ObservableValue<string> _text;
        private readonly IDisposable _currentSubscription;
        private readonly IDisposable _maxSubscription;
        private int _current;
        private int _max;

        /// <exception cref="ArgumentNullException"><paramref name="health"/> is null.</exception>
        public HealthViewModel(IPlayerHealth health)
        {
            if (health == null) throw new ArgumentNullException(nameof(health));

            Current = health.Current;
            Max = health.Max;

            _current = health.Current.Value;
            _max = health.Max.Value;
            _text = new ObservableValue<string>(Format());

            _currentSubscription = health.Current.Subscribe(OnCurrentChanged);
            _maxSubscription = health.Max.Subscribe(OnMaxChanged);
        }

        public IObservableValue<int> Current { get; }
        public IObservableValue<int> Max { get; }
        
        public IObservableValue<string> Text => _text;

        public void Dispose()
        {
            _currentSubscription.Dispose();
            _maxSubscription.Dispose();
        }

        private void OnCurrentChanged(int current)
        {
            _current = current;
            _text.Value = Format();
        }

        private void OnMaxChanged(int max)
        {
            _max = max;
            _text.Value = Format();
        }

        private string Format() => string.Format(CultureInfo.InvariantCulture, TextFormat, _current, _max);
    }
}

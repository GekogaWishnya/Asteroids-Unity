using System;
using System.Globalization;
using Asteroids.Common;
using Asteroids.Core;
using Asteroids.Core.IConfigs;

namespace Asteroids.UI
{
    /// <summary>
    /// What the score looks like on the HUD. <see cref="Text"/> is zero-padded to at least <see cref="Digits"/> digits.
    /// Follows the score until disposed.
    /// </summary>
    public sealed class ScoreViewModel : IDisposable
    {
        private readonly IScoreHudConfig _config;
        private readonly ObservableValue<string> _text;
        private readonly IDisposable _subscription;

        /// <exception cref="ArgumentNullException"><paramref name="score"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> is null.</exception>
        /// <exception cref="InvalidOperationException">The config is invalid.</exception>
        public ScoreViewModel(IScore score, IScoreHudConfig config)
        {
            if (score == null) throw new ArgumentNullException(nameof(score));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            ConfigValidation.ThrowIfInvalid("The score HUD config", ScoreHudConfigValidator.Validate(config));

            _text = new ObservableValue<string>(Format(score.Value.Value));
            _subscription = score.Value.Subscribe(OnScoreChanged);
        }

        public IObservableValue<string> Text => _text;

        public void Dispose() => _subscription.Dispose();

        private void OnScoreChanged(int value) => _text.Value = Format(value);

        private string Format(int value) => string.Format(CultureInfo.InvariantCulture, _config.ScoreFormat, value);
    }
}
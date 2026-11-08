using System;
using System.Globalization;
using Asteroids.Common;
using Asteroids.Core;
using Asteroids.Core.IConfigs;

namespace Asteroids.UI
{
    public sealed class RunEndViewModel : IDisposable
    {
        private readonly IGameSession _session;
        private readonly IRunEndScreenConfig _config;
        private readonly ObservableValue<bool> _isVisible = new(false);
        private readonly ObservableValue<string> _title = new(string.Empty);
        private readonly ObservableValue<string> _scoreText = new(string.Empty);
        private readonly ObservableValue<string> _wavesText = new(string.Empty);
        private readonly IDisposable _subscription;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        /// <exception cref="InvalidOperationException">The config is invalid.</exception>
        public RunEndViewModel(IGameSession session, IRunEndScreenConfig config)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            ConfigValidation.ThrowIfInvalid(
                "The run end screen config",
                RunEndScreenConfigValidator.Validate(config)
            );

            _subscription = session.State.Subscribe(OnStateChanged);
        }

        public IObservableValue<bool> IsVisible => _isVisible;
        public IObservableValue<string> Title => _title;
        public IObservableValue<string> ScoreText => _scoreText;
        public IObservableValue<string> WavesText => _wavesText;

        /// <summary>Starts a new run. Ignored unless the screen is visible.</summary>
        public void Replay()
        {
            if (!_isVisible.Value) return;

            _session.Restart();
        }

        public void Dispose() => _subscription.Dispose();

        private void OnStateChanged(RunState state)
        {
            if (state.Phase != RunPhase.Ended)
            {
                _isVisible.Value = false;
                return;
            }

            var result = state.Result;
            _title.Value = result.Outcome == RunOutcome.Victory ? _config.VictoryTitle : _config.DefeatTitle;
            _scoreText.Value = string.Format(CultureInfo.InvariantCulture, _config.ScoreFormat, result.Score);
            _wavesText.Value = string.Format(CultureInfo.InvariantCulture, _config.WavesFormat,
                result.WavesCleared, result.WaveCount);
            _isVisible.Value = true;
        }
    }
}
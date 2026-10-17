using System;
using System.Globalization;
using Asteroids.Common;
using Asteroids.Core;

namespace Asteroids.UI
{
    /// <summary>
    /// What the score looks like on the HUD. <see cref="Text"/> is zero-padded to at least <see cref="Digits"/> digits.
    /// Follows the score until disposed.
    /// </summary>
    public sealed class ScoreViewModel : IDisposable
    {
        /// <summary>Digits the score is padded to. A longer score is shown in full.</summary>
        public const int Digits = 6;

        private const string Label = "SCORE ";
        private static readonly string NumberFormat = "D" + Digits.ToString(CultureInfo.InvariantCulture);

        private readonly ObservableValue<string> _text;
        private readonly IDisposable _subscription;

        /// <exception cref="ArgumentNullException"><paramref name="score"/> is null.</exception>
        public ScoreViewModel(IScore score)
        {
            if (score == null) throw new ArgumentNullException(nameof(score));

            _text = new ObservableValue<string>(Format(score.Value.Value));
            _subscription = score.Value.Subscribe(OnScoreChanged);
        }
        
        public IObservableValue<string> Text => _text;

        public void Dispose() => _subscription.Dispose();

        private void OnScoreChanged(int value) => _text.Value = Format(value);

        private static string Format(int value) => Label + value.ToString(NumberFormat, CultureInfo.InvariantCulture);
    }
}
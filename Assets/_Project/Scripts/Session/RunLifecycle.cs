using System;
using Asteroids.Common;
using Asteroids.Core;
using Asteroids.Core.IConfigs;

namespace Asteroids.Session
{
    /// <summary>The state machine of one run</summary>
    public sealed class RunLifecycle : ISimulationStep, IRestartable
    {
        private readonly IRunEndConfig _config;
        private readonly IScore _score;
        private readonly IWaveProgress _waves;
        private readonly ObservableValue<RunState> _state = new(RunState.Playing);
        private float _remaining;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        /// <exception cref="InvalidOperationException">The config is invalid; the message lists every problem.</exception>
        public RunLifecycle(IRunEndConfig config, IScore score, IWaveProgress waves)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _score = score ?? throw new ArgumentNullException(nameof(score));
            _waves = waves ?? throw new ArgumentNullException(nameof(waves));

            ConfigValidation.ThrowIfInvalid("The run end config", RunEndConfigValidator.Validate(config));
        }

        public IObservableValue<RunState> State => _state;

        public RestartOrder Order => RestartOrder.Run;

        /// <summary>Ignored unless the run is <see cref="RunPhase.Playing"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="outcome"/> is not defined.</exception>
        public void Finish(RunOutcome outcome)
        {
            var delay = DelayOf(outcome);
            if (_state.Value.Phase != RunPhase.Playing) return;

            var result = new RunResult(outcome, _score.Value.Value, _waves.Cleared, _waves.Total);
            _remaining = delay;

            _state.Value = RunState.Ending(result);
            if (_remaining <= 0f) End();
        }

        public void Tick(float deltaTime)
        {
            if (_state.Value.Phase != RunPhase.Ending) return;

            _remaining -= deltaTime;
            if (_remaining <= 0f) End();
        }

        public void Restart()
        {
            _remaining = 0f;
            _state.Value = RunState.Playing;
        }

        private void End()
        {
            var state = _state.Value;
            if (state.Phase != RunPhase.Ending) return;

            _remaining = 0f;
            _state.Value = RunState.Ended(state.Result);
        }

        private float DelayOf(RunOutcome outcome) => outcome switch
        {
            RunOutcome.Defeat => _config.DefeatDelay,
            RunOutcome.Victory => _config.VictoryDelay,
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Unknown run outcome.")
        };
    }
}
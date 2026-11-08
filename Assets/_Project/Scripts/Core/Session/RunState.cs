using System;

namespace Asteroids.Core
{
    public readonly struct RunState : IEquatable<RunState>
    {
        private readonly RunResult _result;

        private RunState(RunPhase phase, RunResult result)
        {
            Phase = phase;
            _result = result;
        }

        public static RunState Playing => default;

        public static RunState Ending(RunResult result) => new(RunPhase.Ending, result);

        public static RunState Ended(RunResult result) => new(RunPhase.Ended, result);

        public RunPhase Phase { get; }

        public bool HasResult => Phase != RunPhase.Playing;

        /// <exception cref="InvalidOperationException">The run is still <see cref="RunPhase.Playing"/>.</exception>
        public RunResult Result => HasResult
            ? _result
            : throw new InvalidOperationException("A run that is still playing has no result.");

        public bool Equals(RunState other) => Phase == other.Phase && _result.Equals(other._result);

        public override bool Equals(object obj) => obj is RunState other && Equals(other);

        public override int GetHashCode() => HashCode.Combine((int)Phase, _result);

        public override string ToString() => HasResult ? $"{Phase} ({_result})" : Phase.ToString();
    }
}
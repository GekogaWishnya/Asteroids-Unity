using System;

namespace Asteroids.Core
{
    public readonly struct RunResult : IEquatable<RunResult>
    {
        public RunResult(RunOutcome outcome, int score, int wavesCleared, int waveCount)
        {
            Outcome = outcome;
            Score = score;
            WavesCleared = wavesCleared;
            WaveCount = waveCount;
        }

        public RunOutcome Outcome { get; }
        public int Score { get; }
        public int WavesCleared { get; }
        public int WaveCount { get; }

        public bool Equals(RunResult other) =>
            Outcome == other.Outcome && Score == other.Score &&
            WavesCleared == other.WavesCleared && WaveCount == other.WaveCount;

        public override bool Equals(object obj) => obj is RunResult other && Equals(other);

        public override int GetHashCode() => HashCode.Combine((int)Outcome, Score, WavesCleared, WaveCount);

        public override string ToString() => $"{Outcome}, score {Score}, waves {WavesCleared}/{WaveCount}";
    }
}
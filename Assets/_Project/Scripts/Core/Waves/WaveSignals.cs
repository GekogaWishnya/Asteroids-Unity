namespace Asteroids.Core
{
    /// <summary>Fired when the delay of a wave has passed and its enemies start to appear.</summary>
    public readonly struct WaveStarted
    {
        public WaveStarted(int number, int total)
        {
            Number = number;
            Total = total;
        }

        /// <summary>Starts at 1.</summary>
        public int Number { get; }

        public int Total { get; }
    }
    
    public readonly struct WaveCleared
    {
        public WaveCleared(int number) => Number = number;

        /// <summary>Starts at 1.</summary>
        public int Number { get; }
    }
    
    public readonly struct LevelCompleted
    {
        public LevelCompleted(int waveCount) => WaveCount = waveCount;

        public int WaveCount { get; }
    }
}

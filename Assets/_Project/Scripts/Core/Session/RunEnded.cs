namespace Asteroids.Core
{
    /// <summary>Fired once per run, when the end delay has passed and the simulation has been paused.</summary>
    public readonly struct RunEnded
    {
        public RunEnded(RunResult result) => Result = result;

        public RunResult Result { get; }
    }
}
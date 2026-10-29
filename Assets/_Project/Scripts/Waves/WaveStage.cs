namespace Asteroids.Waves
{
    public enum WaveStage
    {
        /// <summary>Counting down the delay before the next wave.</summary>
        Waiting,

        /// <summary>The wave has started and some of its enemies are still to appear.</summary>
        Spawning,

        /// <summary>Everything has appeared. Waiting for the last enemy to be killed.</summary>
        Clearing,

        /// <summary>The last wave is cleared.</summary>
        Completed
    }
}

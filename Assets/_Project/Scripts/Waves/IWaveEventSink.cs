namespace Asteroids.Waves
{
    /// <summary>Where the director reports progress.</summary>
    public interface IWaveEventSink
    {
        void Started(int number, int total);
        void Cleared(int number);
        void Completed(int total);
    }
}

namespace Asteroids.Core
{
    public interface IWaveProgress
    {
        int Cleared { get; }
        int Total { get; }
    }
}
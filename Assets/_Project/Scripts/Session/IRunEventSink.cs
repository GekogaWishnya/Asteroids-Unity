using Asteroids.Core;

namespace Asteroids.Session
{
    public interface IRunEventSink
    {
        void Ended(RunResult result);
    }
}
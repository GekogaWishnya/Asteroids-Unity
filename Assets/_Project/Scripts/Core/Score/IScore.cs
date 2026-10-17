using Asteroids.Common;

namespace Asteroids.Core
{
    public interface IScore
    {
        IObservableValue<int> Value { get; }
    }
}

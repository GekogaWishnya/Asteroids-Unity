using Asteroids.Common;

namespace Asteroids.Core
{
    public interface ILaserStatus
    {
        /// <summary>Energy in the tank, from 0 to 1.</summary>
        IObservableValue<float> Energy { get; }

        /// <summary>Seconds until the tank is full.</summary>
        IObservableValue<float> SecondsToFull { get; }
    }
}

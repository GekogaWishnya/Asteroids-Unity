using System;

namespace Asteroids.Common
{
    /// <summary>Read-only view of a value that tells its subscribers when it changes.</summary>
    public interface IObservableValue<out T>
    {
        T Value { get; }

        /// <summary>
        /// Calls <paramref name="onChanged"/> immediately and then after every change.
        /// Disposing the result stops the notifications.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="onChanged"/> is null.</exception>
        IDisposable Subscribe(Action<T> onChanged);
    }
}

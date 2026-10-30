using System;

namespace Asteroids.Common
{
    /// <summary>
    /// <para>
    /// Read-only view of a value that tells its subscribers when it changes.
    /// </para>
    /// <para>
    /// Subscribers are notified in subscription order. Subscribing and disposing from inside a callback is safe.
    /// </para>
    /// <para>
    /// Setting the value from inside a callback does not notify recursively. Instead, the running pass finishes
    /// and one more pass with the latest value follows, so values set in between are skipped.
    /// </para>
    /// <para>
    /// If a callback throws, the remaining subscribers of that pass are not notified.
    /// </para>
    /// </summary>
    public interface IObservableValue<out T>
    {
        T Value { get; }

        /// <summary>
        /// Calls <paramref name="onChanged"/> immediately and then after every change.
        /// Disposing the result stops the notifications.
        /// If the immediate call throws, the subscription is removed and the exception propagates.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="onChanged"/> is null.</exception>
        IDisposable Subscribe(Action<T> onChanged);
    }
}

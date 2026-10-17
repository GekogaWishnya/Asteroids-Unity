using System;
using System.Collections.Generic;

namespace Asteroids.Common
{
    /// <summary>
    /// <para>
    /// A value that notifies its subscribers when it changes. The owner keeps this type and
    /// hands out <see cref="IObservableValue{T}"/>, so only the owner can write.
    /// </para>
    /// <para>
    /// Subscribers are notified in subscription order. Subscribing and disposing from inside a callback is safe.
    /// </para>
    /// <para>
    /// Setting the value from inside a callback does not notify recursively. Instead, the running pass finishes
    /// and one more pass with the latest value follows.
    /// </para>
    /// <para>
    /// If a callback throws, the exception reaches the caller of the setter, the remaining subscribers of
    /// that pass are not notified, and the value stays changed. Not thread-safe.
    /// </para>
    /// </summary>
    public sealed class ObservableValue<T> : IObservableValue<T>
    {
        private static readonly EqualityComparer<T> Comparer = EqualityComparer<T>.Default;

        private Subscription[] _subscriptions = Array.Empty<Subscription>();
        private T _value;
        private bool _isNotifying;
        private bool _hasPendingPass;

        public ObservableValue(T initialValue = default) => _value = initialValue;

        public T Value
        {
            get => _value;
            set
            {
                if (Comparer.Equals(_value, value)) return;

                _value = value;
                Notify();
            }
        }

        /// <exception cref="ArgumentNullException"><paramref name="onChanged"/> is null.</exception>
        public IDisposable Subscribe(Action<T> onChanged)
        {
            if (onChanged == null) throw new ArgumentNullException(nameof(onChanged));

            var subscription = new Subscription(this, onChanged);
            Append(subscription);

            try
            {
                onChanged(_value);
            }
            catch
            {
                subscription.Dispose();
                throw;
            }

            return subscription;
        }

        private void Notify()
        {
            if (_isNotifying)
            {
                _hasPendingPass = true;
                return;
            }

            _isNotifying = true;
            try
            {
                do
                {
                    _hasPendingPass = false;

                    var value = _value;
                    foreach (var subscription in _subscriptions)
                    {
                        if (!subscription.IsDisposed) subscription.Callback(value);
                    }
                } while (_hasPendingPass);
            }
            finally
            {
                _isNotifying = false;
            }
        }

        private void Append(Subscription subscription)
        {
            var next = new Subscription[_subscriptions.Length + 1];
            Array.Copy(_subscriptions, next, _subscriptions.Length);
            next[^1] = subscription;
            _subscriptions = next;
        }

        private void Remove(Subscription subscription)
        {
            var index = Array.IndexOf(_subscriptions, subscription);
            if (index < 0) return;

            var next = new Subscription[_subscriptions.Length - 1];
            Array.Copy(_subscriptions, 0, next, 0, index);
            Array.Copy(_subscriptions, index + 1, next, index, next.Length - index);
            _subscriptions = next;
        }

        private sealed class Subscription : IDisposable
        {
            private ObservableValue<T> _owner;

            public Subscription(ObservableValue<T> owner, Action<T> callback)
            {
                _owner = owner;
                Callback = callback;
            }

            public Action<T> Callback { get; }
            public bool IsDisposed => _owner == null;

            public void Dispose()
            {
                var owner = _owner;
                if (owner == null) return;

                _owner = null;
                owner.Remove(this);
            }
        }
    }
}

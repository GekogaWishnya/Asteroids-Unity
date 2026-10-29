using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace Asteroids.Common
{
    /// <summary>
    /// <para>
    /// Reuses instances of <typeparamref name="T"/>. <see cref="Get"/> creates a new instance if there is not enough.
    /// </para>
    /// <para>
    /// The pool keeps at most <c>maxIdle</c> released instances. Any further released instance is passed
    /// to the destroy callback instead.
    /// </para>
    /// <para>
    /// A released or prewarmed instance whose callback throws is destroyed, and the failure propagates.
    /// Exceptions of the destroy callback during that cleanup are hidden so they cannot mask the original failure.
    /// </para>
    /// </summary>
    public sealed class ObjectPool<T> where T : class
    {
        private readonly Func<T> _create;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly Action<T> _onDestroy;
        private readonly int _maxIdle;
        private readonly Stack<T> _idle = new();
        private readonly HashSet<T> _active = new(ReferenceComparer.Instance);

        /// <param name="create">Creates a new instance. Must not return null.</param>
        /// <param name="onGet">Runs on every instance handed out by <see cref="Get"/>.</param>
        /// <param name="onRelease">Runs on every instance returned through <see cref="Release"/> and on prewarmed ones.</param>
        /// <param name="onDestroy">Runs on every instance the pool lets go of.</param>
        /// <param name="maxIdle">Most released instances the pool keeps.</param>
        /// <exception cref="ArgumentNullException"><paramref name="create"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxIdle"/> is negative.</exception>
        public ObjectPool(
            Func<T> create,
            Action<T> onGet = null,
            Action<T> onRelease = null,
            Action<T> onDestroy = null,
            int maxIdle = int.MaxValue)
        {
            if (maxIdle < 0) throw new ArgumentOutOfRangeException(nameof(maxIdle), "Max idle must not be negative.");

            _create = create ?? throw new ArgumentNullException(nameof(create));
            _onGet = onGet;
            _onRelease = onRelease;
            _onDestroy = onDestroy;
            _maxIdle = maxIdle;
        }

        /// <summary>Instances handed out and not yet released.</summary>
        public int CountActive => _active.Count;

        /// <summary>Released instances the pool is holding.</summary>
        public int CountIdle => _idle.Count;

        /// <exception cref="InvalidOperationException">The <see cref="Create"/> returned null.</exception>
        public T Get()
        {
            var item = _idle.Count > 0 ? _idle.Pop() : Create();
            _active.Add(item);

            try
            {
                _onGet?.Invoke(item);
            }
            catch
            {
                _active.Remove(item);
                DestroyQuietly(item);
                throw;
            }

            return item;
        }

        /// <exception cref="ArgumentNullException"><paramref name="item"/> is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// The item was not handed out by this pool or has already been released.
        /// </exception>
        public void Release(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (!_active.Remove(item))
                throw new InvalidOperationException(
                    "The item was not handed out by this pool or has already been released.");

            try
            {
                _onRelease?.Invoke(item);
            }
            catch
            {
                DestroyQuietly(item);
                throw;
            }

            if (_idle.Count < _maxIdle) _idle.Push(item);
            else Destroy(item);
        }

        /// <summary>
        /// Creates instances until at least <paramref name="count"/> are idle, never going above
        /// <c>maxIdle</c> (the surplus would be destroyed at once, so it is not created).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
        public void Prewarm(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "Count must not be negative.");

            var target = Math.Min(count, _maxIdle);
            while (_idle.Count < target)
            {
                var item = Create();

                try
                {
                    _onRelease?.Invoke(item);
                }
                catch
                {
                    DestroyQuietly(item);
                    throw;
                }

                _idle.Push(item);
            }
        }

        /// <summary>
        /// Destroys all idle instances. Active ones are not touched. If destroy callbacks throw,
        /// the remaining instances are still destroyed and the first exception is rethrown afterward.
        /// </summary>
        public void Clear()
        {
            ExceptionDispatchInfo first = null;

            while (_idle.Count > 0)
            {
                try
                {
                    Destroy(_idle.Pop());
                }
                catch (Exception exception)
                {
                    first ??= ExceptionDispatchInfo.Capture(exception);
                }
            }

            first?.Throw();
        }

        private T Create() =>
            _create() ?? throw new InvalidOperationException("The create callback returned null.");

        private void Destroy(T item) => _onDestroy?.Invoke(item);

        private void DestroyQuietly(T item)
        {
            try
            {
                Destroy(item);
            }
            catch
            {
                // The failure that led here matters more than a failure of the cleanup.
            }
        }

        private sealed class ReferenceComparer : IEqualityComparer<T>
        {
            public static readonly ReferenceComparer Instance = new();

            public bool Equals(T x, T y) => ReferenceEquals(x, y);
            public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }
}

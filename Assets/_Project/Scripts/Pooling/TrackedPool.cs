using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Asteroids.Common;

namespace Asteroids.Pooling
{
    /// <summary>
    /// A pool of objects and the set of those in play. A finished object goes back to the pool by itself.
    /// The pool grows on demand, so a spawn never fails for lack of an idle object.
    /// </summary>
    public sealed class TrackedPool<T> where T : class, IPooledObject<T>
    {
        private readonly Func<T> _create;
        private readonly ObjectPool<T> _pool;
        private readonly HashSet<T> _inPlay = new(ReferenceComparer.Instance);

        /// <param name="create">Creates one idle object.</param>
        /// <exception cref="ArgumentNullException"><paramref name="create"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxIdle"/> is negative.</exception>
        public TrackedPool(Func<T> create, int maxIdle)
        {
            _create = create ?? throw new ArgumentNullException(nameof(create));
            _pool = new ObjectPool<T>(CreateInstance, onDestroy: DestroyInstance, maxIdle: maxIdle);
        }

        public int InPlayCount => _inPlay.Count;

        public void Prewarm(int count) => _pool.Prewarm(count);

        /// <summary>Takes an idle object and lets <paramref name="enter"/> put it in play.
        /// If <paramref name="enter"/> throws, the object goes back to the pool and the failure is rethrown.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="enter"/> is null.</exception>
        public void Spawn(Action<T> enter)
        {
            if (enter == null) throw new ArgumentNullException(nameof(enter));
            Spawn(enter, static (item, action) => action(item));
        }

        /// <inheritdoc cref="Spawn(Action{T})"/>
        public void Spawn<TState>(TState state, Action<T, TState> enter)
        {
            if (enter == null) throw new ArgumentNullException(nameof(enter));

            var item = _pool.Get();
            _inPlay.Add(item);

            try
            {
                enter(item, state);
            }
            catch
            {
                if (_inPlay.Remove(item)) _pool.Release(item);
                throw;
            }
        }

        /// <summary>Takes every object in play out of play without raising death events or such.</summary>
        public void DespawnAll()
        {
            if (_inPlay.Count == 0) return;

            var snapshot = new T[_inPlay.Count];
            _inPlay.CopyTo(snapshot);

            var failures = new FailureCollector();
            foreach (var item in snapshot)
            {
                try
                {
                    item.Despawn();
                }
                catch (Exception e)
                {
                    failures.Record(e);
                }
            }

            failures.ThrowIfFailed();
        }

        private T CreateInstance()
        {
            var item = _create() ?? throw new InvalidOperationException("The object factory returned null.");
            item.Finished += OnFinished;
            return item;
        }

        private void DestroyInstance(T item)
        {
            item.Finished -= OnFinished;
            item.Dispose();
        }

        private void OnFinished(T item)
        {
            if (_inPlay.Remove(item)) _pool.Release(item);
        }

        private sealed class ReferenceComparer : IEqualityComparer<T>
        {
            public static readonly ReferenceComparer Instance = new();

            public bool Equals(T x, T y) => ReferenceEquals(x, y);
            public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }
}
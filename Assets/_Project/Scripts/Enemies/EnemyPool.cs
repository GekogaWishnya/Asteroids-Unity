using System;
using System.Collections.Generic;
using Asteroids.Common;

namespace Asteroids.Enemies
{
    /// <summary>
    /// The pooled objects of one enemy type and the set of those in play. A finished enemy goes back to the
    /// pool by itself. The pool grows on demand, so <see cref="Launch"/> never fails for lack of an idle enemy.
    /// </summary>
    internal sealed class EnemyPool<T> where T : class, IPooledEnemy<T>
    {
        private readonly Func<T> _create;
        private readonly ObjectPool<T> _pool;
        private readonly HashSet<T> _inPlay = new(ReferenceComparer.Instance);

        /// <param name="create">Creates one idle enemy.</param>
        /// <exception cref="ArgumentNullException"><paramref name="create"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxIdle"/> is negative.</exception>
        public EnemyPool(Func<T> create, int maxIdle)
        {
            _create = create ?? throw new ArgumentNullException(nameof(create));
            _pool = new ObjectPool<T>(CreateInstance, onDestroy: DestroyInstance, maxIdle: maxIdle);
        }

        public int AliveCount => _inPlay.Count;

        public void Prewarm(int count) => _pool.Prewarm(count);

        /// <summary>Takes an idle enemy and lets <paramref name="enter"/> put it in play.</summary>
        /// <remarks>If <paramref name="enter"/> throws, the enemy goes back to the pool and the failure is rethrown.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="enter"/> is null.</exception>
        public void Launch(Action<T> enter)
        {
            if (enter == null) throw new ArgumentNullException(nameof(enter));

            var enemy = _pool.Get();
            _inPlay.Add(enemy);

            try
            {
                enter(enemy);
            }
            catch
            {
                if (_inPlay.Remove(enemy)) _pool.Release(enemy);
                throw;
            }
        }

        /// <summary>Takes every enemy in play out of play without a death event.</summary>
        public void DespawnAll()
        {
            if (_inPlay.Count == 0) return;

            var snapshot = new T[_inPlay.Count];
            _inPlay.CopyTo(snapshot);

            var failures = new FailureCollector();
            foreach (var enemy in snapshot)
            {
                try
                {
                    enemy.Despawn();
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
            var enemy = _create() ?? throw new InvalidOperationException("The enemy factory returned null.");
            enemy.Finished += OnFinished;
            return enemy;
        }

        private void DestroyInstance(T enemy)
        {
            enemy.Finished -= OnFinished;
            enemy.Dispose();
        }

        private void OnFinished(T enemy)
        {
            if (_inPlay.Remove(enemy)) _pool.Release(enemy);
        }

        private sealed class ReferenceComparer : IEqualityComparer<T>
        {
            public static readonly ReferenceComparer Instance = new();

            public bool Equals(T x, T y) => ReferenceEquals(x, y);
            public int GetHashCode(T obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }
}
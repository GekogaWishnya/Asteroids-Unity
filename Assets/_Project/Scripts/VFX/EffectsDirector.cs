using System;
using System.Collections.Generic;
using Asteroids.Common;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using UnityEngine;
using Zenject;

namespace Asteroids.VFX
{
    /// <summary>
    /// Facade of the VFX system. Plays the configured effects for gameplay signals and owns their pools.
    /// </summary>
    public sealed class EffectsDirector : IEffectsPause, IInitializable, IDisposable
    {
        private readonly Dictionary<IEffectConfig, EffectPool> _pools = new();
        private readonly Dictionary<EnemyTypeId, Feedback> _enemies = new();
        private readonly HashSet<EnemyTypeId> _reportedUnknown = new();
        private readonly Feedback _ship;
        private readonly IErrorLog _errors;
        private bool _isDisposed;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        /// <exception cref="InvalidOperationException">The config is invalid.</exception>
        public EffectsDirector(IEffectsConfig config, IEffectFactory factory, IErrorLog errors)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            _errors = errors ?? throw new ArgumentNullException(nameof(errors));

            ConfigValidation.ThrowIfInvalid("The effects config", EffectsConfigValidator.Validate(config));

            _ship = CreateFeedback(config.Ship, factory);
            foreach (var entry in config.Enemies ?? Array.Empty<IEnemyFeedbackConfig>())
                _enemies.Add(entry.Type, CreateFeedback(entry, factory));
        }

        public bool IsPaused { get; private set; }

        public void Initialize()
        {
            foreach (var pool in _pools.Values) Guard(pool.Prewarm, "Prewarming an effect pool");
        }

        public void OnShipDamaged(ShipDamaged signal) =>
            Guard(() => _ship.Damaged(signal.Position, signal.IsFatal), "Playing the ship hit effect");

        public void OnShipDestroyed(ShipDestroyed signal) =>
            Guard(() =>
            {
                if (signal.Entity == null) throw new ArgumentException("The destroyed ship has no entity.");
                _ship.Died(signal.Entity.Pose.Position);
            }, "Playing the ship death effect");

        public void OnEnemyDamaged(EnemyDamaged signal) =>
            Guard(() => Find(signal.Type)?.Damaged(signal.Position, signal.IsFatal),
                $"Playing the hit effect of enemy type '{signal.Type}'");

        public void OnEnemyDied(EnemyDied signal) =>
            Guard(() => Find(signal.Type)?.Died(signal.Position),
                $"Playing the death effect of enemy type '{signal.Type}'");

        public void SetPaused(bool paused)
        {
            if (IsPaused == paused) return;

            IsPaused = paused;
            foreach (var pool in _pools.Values) Guard(() => pool.SetPaused(paused), "Pausing an effect pool");
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            foreach (var pool in _pools.Values) Report(pool.Dispose, "Disposing an effect pool");
            _pools.Clear();
        }

        private Feedback CreateFeedback(IFeedbackConfig config, IEffectFactory factory) =>
            new(PoolOf(config.Hit, factory), config.PlayHitOnFatal, PoolOf(config.Death, factory));

        /// <returns>The pool shared by every use of <paramref name="effect"/>. Null when there is no effect.</returns>
        private EffectPool PoolOf(IEffectConfig effect, IEffectFactory factory)
        {
            if (effect == null) return null;
            if (_pools.TryGetValue(effect, out var pool)) return pool;

            pool = new EffectPool(effect, factory);
            _pools.Add(effect, pool);

            return pool;
        }

        private Feedback Find(EnemyTypeId type)
        {
            if (_enemies.TryGetValue(type, out var feedback)) return feedback;

            if (_reportedUnknown.Add(type))
                throw new InvalidOperationException(
                    $"Enemy type '{type}' has no effects entry, although every spawnable type should have one.");
            return null;
        }

        private void Guard(Action action, string context)
        {
            if (!_isDisposed) Report(action, context);
        }

        private void Report(Action action, string context)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                _errors.Report(e, context);
            }
        }

        private sealed class Feedback
        {
            private readonly EffectPool _hit;
            private readonly bool _hitOnFatal;
            private readonly EffectPool _death;

            public Feedback(EffectPool hit, bool hitOnFatal, EffectPool death)
            {
                _hit = hit;
                _hitOnFatal = hitOnFatal;
                _death = death;
            }

            public void Damaged(Vector2 position, bool isFatal)
            {
                if (_hit != null && (!isFatal || _hitOnFatal)) _hit.Play(position);
            }

            public void Died(Vector2 position) => _death?.Play(position);
        }
    }
}
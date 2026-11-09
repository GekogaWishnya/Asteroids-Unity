using System;
using System.Collections.Generic;
using Asteroids.Common;
using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.VFX
{
    public sealed class EffectPool : IDisposable
    {
        private readonly IEffectConfig _config;
        private readonly ObjectPool<IEffect> _pool;
        private readonly List<IEffect> _playing = new();
        private bool _isPaused;
        private bool _isDisposed;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        /// <exception cref="InvalidOperationException">The config is invalid.</exception>
        public EffectPool(IEffectConfig config, IEffectFactory factory)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            var problems = new List<string>();
            EffectsConfigValidator.ValidateEffect($"Effect '{config.Name}'", config, problems);
            ConfigValidation.ThrowIfInvalid($"Effect '{config.Name}'", problems);

            _pool = new ObjectPool<IEffect>(
                () => Create(factory),
                onRelease: effect => effect.Stop(),
                onDestroy: Destroy,
                maxIdle: config.PoolMaxIdle);
        }

        public int PlayingCount => _playing.Count;

        public void Prewarm() => _pool.Prewarm(_config.PoolPrewarm);

        /// <exception cref="ObjectDisposedException">The pool is disposed.</exception>
        public void Play(Vector2 position)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(EffectPool));

            while (_playing.Count >= _config.MaxActive) Recycle(_playing[0]);

            var effect = _pool.Get();
            _playing.Add(effect);

            try
            {
                effect.Play(position, _config.Scale);
                if (_isPaused) effect.Pause();
            }
            catch
            {
                Recycle(effect);
                throw;
            }
        }

        public void SetPaused(bool paused)
        {
            if (_isPaused == paused) return;

            _isPaused = paused;
            foreach (var effect in _playing.ToArray())
            {
                if (paused) effect.Pause();
                else effect.Resume();
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            var failures = new FailureCollector();
            foreach (var effect in _playing.ToArray())
            {
                try
                {
                    Recycle(effect);
                }
                catch (Exception e)
                {
                    failures.Record(e);
                }
            }

            try
            {
                _pool.Clear();
            }
            catch (Exception e)
            {
                failures.Record(e);
            }

            failures.ThrowIfFailed();
        }

        private IEffect Create(IEffectFactory factory)
        {
            var effect = factory.Create(_config)
                         ?? throw new InvalidOperationException(
                             $"The factory made no object for effect '{_config.Name}'.");
            effect.Finished += OnFinished;
            return effect;
        }

        private void Destroy(IEffect effect)
        {
            effect.Finished -= OnFinished;
            effect.Dispose();
        }

        private void OnFinished(IEffect effect)
        {
            if (_playing.Contains(effect)) Recycle(effect);
        }

        private void Recycle(IEffect effect)
        {
            _playing.Remove(effect);
            _pool.Release(effect);
        }
    }
}
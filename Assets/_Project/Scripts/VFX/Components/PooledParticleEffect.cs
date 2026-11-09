using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Asteroids.VFX.Components
{
    internal sealed class PooledParticleEffect : IEffect
    {
        private readonly ParticleEffect _effect;
        private readonly Transform _transform;
        private readonly Transform _idleRoot;
        private readonly Transform _activeRoot;
        private readonly Vector3 _baseScale;
        private bool _isPlaying;

        public PooledParticleEffect(ParticleEffect effect, Transform idleRoot, Transform activeRoot)
        {
            _effect = effect;
            _transform = effect.transform;
            _idleRoot = idleRoot;
            _activeRoot = activeRoot;
            _baseScale = _transform.localScale;
            _effect.Stopped += OnStopped;
        }

        public event Action<IEffect> Finished;

        public void Play(Vector2 position, float scale)
        {
            if (!_transform) throw new InvalidOperationException("The effect object was destroyed.");

            _transform.SetParent(_activeRoot, false);
            _transform.SetPositionAndRotation(
                new Vector3(position.x, position.y, _transform.position.z), _transform.rotation);
            _transform.localScale = _baseScale * scale;

            var system = _effect.Particles;
            system.Clear(true);
            system.Play(true);
            _isPlaying = true;
        }

        public void Stop()
        {
            _isPlaying = false;
            if (!_transform) return;

            _effect.Particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _transform.SetParent(_idleRoot, false);
        }

        public void Pause()
        {
            if (_isPlaying && _transform) _effect.Particles.Pause(true);
        }

        public void Resume()
        {
            if (_isPlaying && _transform && _effect.Particles.isPaused) _effect.Particles.Play(true);
        }

        public void Dispose()
        {
            _isPlaying = false;
            Finished = null;
            if (!_transform) return;

            _effect.Stopped -= OnStopped;
            Object.Destroy(_transform.gameObject);
        }

        private void OnStopped()
        {
            // A stop we caused, or a late callback for a play that was cut short and has started again.
            if (!_isPlaying || _effect.Particles.IsAlive(true)) return;

            _isPlaying = false;
            Finished?.Invoke(this);
        }
    }
}
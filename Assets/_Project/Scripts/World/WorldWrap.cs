using System;
using System.Collections.Generic;
using Asteroids.Common;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Physics;
using UnityEngine;

namespace Asteroids.World
{
    /// <summary>
    /// Wraps every registered pose around the gameplay area. Runs in <see cref="SimulationPhase.Constraint"/>.
    /// </summary>
    public sealed class WorldWrap : IWrapRegistry, ISimulationStep
    {
        private const float EdgeInset = 0.001f;

        private readonly IWorldBounds _bounds;
        private readonly ISpawnEntryConfig _entry;
        private readonly HashSet<IPose> _registered = new();
        private readonly DeferredList<IPose> _poses = new();

        /// <summary>Seconds each entering pose has spent outside the area so far.</summary>
        private readonly Dictionary<IPose, float> _entering = new();

        /// <exception cref="ArgumentNullException"><paramref name="bounds"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="entry"/> is null.</exception>
        /// <exception cref="InvalidOperationException">The spawn entry config is invalid.</exception>
        public WorldWrap(IWorldBounds bounds, ISpawnEntryConfig entry)
        {
            _bounds = bounds ?? throw new ArgumentNullException(nameof(bounds));
            _entry = entry ?? throw new ArgumentNullException(nameof(entry));

            ConfigValidation.ThrowIfInvalid("The spawn entry config", SpawnEntryConfigValidator.Validate(entry));
        }

        public int Count => _registered.Count;

        public int EnteringCount => _entering.Count;

        public IDisposable Register(IPose pose) => Add(pose, entering: false);

        public IDisposable RegisterEntering(IPose pose) => Add(pose, entering: true);

        public void Tick(float deltaTime)
        {
            _poses.Lock();
            try
            {
                var poses = _poses.Items;
                for (var i = 0; i < poses.Count; i++)
                {
                    var pose = poses[i];
                    if (IsStillEntering(pose, deltaTime)) continue;

                    pose.Position = _bounds.Wrap(pose.Position);
                }
            }
            finally
            {
                _poses.Unlock();
            }
        }

        private IDisposable Add(IPose pose, bool entering)
        {
            if (pose == null) throw new ArgumentNullException(nameof(pose));
            if (!_registered.Add(pose)) throw new InvalidOperationException("The pose already wraps.");

            _poses.Add(pose);
            if (entering) _entering[pose] = 0f;

            return new DisposableAction(() =>
            {
                if (!_registered.Remove(pose)) return;

                _poses.Remove(pose);
                _entering.Remove(pose);
            });
        }

        private bool IsStillEntering(IPose pose, float deltaTime)
        {
            if (!_entering.TryGetValue(pose, out var outside)) return false;

            if (_bounds.Rect.Contains(pose.Position))
            {
                _entering.Remove(pose);
                return false;
            }

            outside += deltaTime;
            if (outside >= _entry.MaxEnteringSeconds)
            {
                _entering.Remove(pose);
                pose.Position = OntoNearestEdge(pose.Position);
                return true;
            }

            _entering[pose] = outside;
            return true;
        }

        private Vector2 OntoNearestEdge(Vector2 position)
        {
            var rect = _bounds.Rect;
            return new Vector2(
                Mathf.Clamp(position.x, rect.xMin, rect.xMax - EdgeInset),
                Mathf.Clamp(position.y, rect.yMin, rect.yMax - EdgeInset));
        }
    }
}
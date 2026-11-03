using System;
using System.Collections.Generic;
using Asteroids.Common;
using Asteroids.Core;
using Asteroids.Physics;

namespace Asteroids.World
{
    /// <summary>
    /// Wraps every registered pose around the gameplay area. Runs in <see cref="SimulationPhase.Constraint"/>.
    /// </summary>
    public sealed class WorldWrap : IWrapRegistry, ISimulationStep
    {
        private readonly IWorldBounds _bounds;
        private readonly HashSet<IPose> _registered = new();
        private readonly DeferredList<IPose> _poses = new();

        /// <exception cref="ArgumentNullException"><paramref name="bounds"/> is null.</exception>
        public WorldWrap(IWorldBounds bounds) =>
            _bounds = bounds ?? throw new ArgumentNullException(nameof(bounds));

        public int Count => _registered.Count;

        public IDisposable Register(IPose pose)
        {
            if (pose == null) throw new ArgumentNullException(nameof(pose));
            if (!_registered.Add(pose)) throw new InvalidOperationException("The pose already wraps.");

            _poses.Add(pose);
            return new DisposableAction(() =>
            {
                if (_registered.Remove(pose)) _poses.Remove(pose);
            });
        }

        public void Tick(float deltaTime)
        {
            _poses.Lock();
            try
            {
                var poses = _poses.Items;
                for (var i = 0; i < poses.Count; i++)
                {
                    var pose = poses[i];
                    pose.Position = _bounds.Wrap(pose.Position);
                }
            }
            finally
            {
                _poses.Unlock();
            }
        }
    }
}

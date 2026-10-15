using System;
using Asteroids.Collisions;
using Asteroids.Core;
using Asteroids.Physics;

namespace Asteroids.World
{
    /// <summary>
    /// Wraps entities around the gameplay area. Runs inside the Collision phase, so it moves the pose
    /// and never touches Unity objects. Collisions are resolved before it.
    /// </summary>
    public sealed class WorldWrapHandler
    {
        private readonly IWorldBounds _bounds;

        public WorldWrapHandler(IWorldBounds bounds) =>
            _bounds = bounds ?? throw new ArgumentNullException(nameof(bounds));

        public void OnCollisionEntered(CollisionEntered signal) => Handle(signal.Self, signal.Other);
        public void OnCollisionStayed(CollisionStayed signal) => Handle(signal.Self, signal.Other);
        
        private void Handle(ICollider self, ICollider other)
        {
            if (!ReferenceEquals(self.Owner, _bounds)) return;
            if (other.Owner is not IEntity entity) return;

            var pose = entity.Pose;
            pose.Position = _bounds.Wrap(pose.Position);
        }
    }
}

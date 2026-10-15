using System;
using Asteroids.Core;

namespace Asteroids.Physics
{
    /// <summary>Pure identity of a simulated thing.</summary>
    public sealed class Entity : IEntity
    {
        public Entity(IPose pose) => Pose = pose ?? throw new ArgumentNullException(nameof(pose));

        public IPose Pose { get; }
    }
}

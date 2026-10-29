using Asteroids.Core;
using Asteroids.Core.IConfigs;

namespace Asteroids.Player.Tests
{
    internal sealed class FakeCollisionIgnore : ICollisionIgnore
    {
        public CollisionLayer Ignored { get; private set; }

        public void Ignore(CollisionLayer layers) => Ignored = layers;

        public void Restore() => Ignored = CollisionLayer.None;
    }
}

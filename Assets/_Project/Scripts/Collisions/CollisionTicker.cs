using Zenject;

namespace Asteroids.Collisions
{
    public sealed class CollisionTicker : ILateTickable
    {
        private readonly ICollisionSystem _system;

        public CollisionTicker(ICollisionSystem system) => _system = system;

        public void LateTick() => _system.Step();
    }
}
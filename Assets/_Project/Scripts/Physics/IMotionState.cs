using UnityEngine;

namespace Asteroids.Physics
{
    public interface IMotionState
    {
        Vector2 Velocity { get; }
        float Speed { get; }
    }
}

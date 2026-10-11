using UnityEngine;

namespace Asteroids.Core
{
    /// Read-only view
    public interface IMotionState
    {
        Vector2 Velocity { get; }
        float Speed { get; }
    }
    
    public interface IMotion : IMotionState
    {
        new Vector2 Velocity { get; set; }
        void AddVelocity(Vector2 delta);
    }
}
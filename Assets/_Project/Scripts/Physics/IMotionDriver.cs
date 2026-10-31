using UnityEngine;

namespace Asteroids.Physics
{
    public interface IMotionDriver
    {
        void Accelerate(Vector2 acceleration); 
        void AddVelocity(Vector2 delta);
    }
}
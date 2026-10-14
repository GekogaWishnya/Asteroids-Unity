using UnityEngine;

namespace Asteroids.Physics
{
    public interface IMotionDriver
    {
        void Accelerate(Vector2 acceleration); 
        void ApplyImpulse(Vector2 impulse);
    }
}

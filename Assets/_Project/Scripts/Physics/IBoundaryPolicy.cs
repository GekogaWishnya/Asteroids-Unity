using UnityEngine;

namespace Asteroids.Physics
{
    public interface IBoundaryPolicy
    {
        Vector2 Apply(Vector2 position, float radius);
    }
}

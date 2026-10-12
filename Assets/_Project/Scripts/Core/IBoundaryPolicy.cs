using UnityEngine;

namespace Asteroids.Core
{
    public interface IBoundaryPolicy
    { Vector2 Apply(Vector2 position, float radius); }
}

using UnityEngine;

namespace Asteroids.Enemies
{
    public interface IEnemyDeathHandler
    {
        void Died(Vector2 position);
    }
}

using UnityEngine;

namespace Asteroids.Enemies
{
    internal interface IEnemyDeathHandler
    {
        void Died(Vector2 position);
    }
}
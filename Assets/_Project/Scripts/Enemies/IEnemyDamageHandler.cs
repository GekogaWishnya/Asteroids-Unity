using UnityEngine;

namespace Asteroids.Enemies
{
    internal interface IEnemyDamageHandler
    {
        /// <param name="position">Where the enemy was when it was hit.</param>
        /// <param name="isFatal">Whether the hit took the last of its health.</param>
        void Damaged(Vector2 position, bool isFatal);
    }
}
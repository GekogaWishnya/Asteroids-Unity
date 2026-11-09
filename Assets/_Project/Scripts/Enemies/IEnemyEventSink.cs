using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Enemies
{
    internal interface IEnemyEventSink
    {
        void Damaged(EnemyTypeId type, Vector2 position, bool isFatal);
        void Died(EnemyTypeId type, Vector2 position);
    }
}
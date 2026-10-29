using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Enemies
{
    public interface IAsteroidLauncher
    {
        void Launch(EnemyTypeId type, Vector2 position);
    }
}

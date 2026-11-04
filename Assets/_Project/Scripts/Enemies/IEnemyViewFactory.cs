using Asteroids.Core;
using Asteroids.Core.IConfigs;

namespace Asteroids.Enemies
{
    public interface IEnemyViewFactory
    {
        IPooledView Create(EnemyTypeId type);
    }
}
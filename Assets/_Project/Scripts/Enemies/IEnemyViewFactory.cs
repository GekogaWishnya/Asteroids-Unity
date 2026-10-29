using Asteroids.Core.IConfigs;

namespace Asteroids.Enemies
{
    public interface IEnemyViewFactory
    {
        IEnemyView Create(EnemyTypeId type);
    }
}

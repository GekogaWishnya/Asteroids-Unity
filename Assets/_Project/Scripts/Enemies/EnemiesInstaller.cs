using Asteroids.Core;
using Zenject;

namespace Asteroids.Enemies
{
    /// <summary>
    /// Binds <see cref="IEnemyData"/> and the spawners of every kind of enemy.
    /// Requires the host installer to bind <see cref="Asteroids.Core.IConfigs.IAsteroidConfig"/>,
    /// <see cref="IEnemyViewFactory"/> and <see cref="Asteroids.Common.IRandom"/>, and requires
    /// <see cref="ISimulationLoop"/>, <see cref="IDamageRegistry"/>, <see cref="IScoreSink"/> and
    /// <see cref="Asteroids.Physics.IWorldBounds"/> to be bound by the simulation, damage, scoring and
    /// world installers.
    /// </summary>
    public sealed class EnemiesInstaller : Installer<EnemiesInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<AsteroidSpawnerBuilder>().AsSingle();

            Container.Bind<IEnemySpawner>()
                .FromMethodMultiple(context => context.Container.Resolve<AsteroidSpawnerBuilder>().Spawners)
                .AsCached();

            // Zenject injects List<T>, not IEnumerable<T>, so the list is resolved explicitly.
            Container.Bind<IEnemyData>()
                .FromMethod(context => new EnemyData(context.Container.ResolveAll<IEnemySpawner>()))
                .AsSingle();
        }
    }
}
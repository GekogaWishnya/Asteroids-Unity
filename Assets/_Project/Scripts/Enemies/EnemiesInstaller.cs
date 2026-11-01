using Asteroids.Core;
using Zenject;

namespace Asteroids.Enemies
{
    /// <summary>
    /// Binds <see cref="IEnemyData"/> and the spawners of every kind of enemy.
    /// Requires the host installer to bind <see cref="Asteroids.Core.IConfigs.IAsteroidConfig"/>,
    /// <see cref="Asteroids.Core.IConfigs.IUfoConfig"/>, <see cref="IPlayerTarget"/>,
    /// <see cref="IEnemyViewFactory"/>, <see cref="Asteroids.Common.IRandom"/>,
    /// <see cref="ISimulationLoop"/>, <see cref="IDamageRegistry"/>, <see cref="IScoreSink"/>,
    /// <see cref="Asteroids.Physics.IWorldBounds"/> and <see cref="IKineticBodyRegistry"/> to be bound
    /// by the simulation, collision, damage, scoring, player and world installers.
    /// </summary>
    public sealed class EnemiesInstaller : Installer<EnemiesInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<AsteroidSpawnerBuilder>().AsSingle();

            Container.Bind<UfoSpawnerBuilder>().AsSingle();

            Container.Bind<IEnemySpawner>()
                .FromMethodMultiple(context => context.Container.Resolve<AsteroidSpawnerBuilder>().Spawners)
                .AsCached();

            Container.Bind<IEnemySpawner>()
                .FromMethodMultiple(context => context.Container.Resolve<UfoSpawnerBuilder>().Spawners)
                .AsCached();

            // Zenject injects List<T>, not IEnumerable<T>, so the list is resolved explicitly.
            Container.Bind<IEnemyData>()
                .FromMethod(context => new EnemyData(context.Container.ResolveAll<IEnemySpawner>()))
                .AsSingle();
        }
    }
}

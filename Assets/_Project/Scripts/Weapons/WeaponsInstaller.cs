using Asteroids.Collisions;
using Asteroids.Weapons.Laser;
using Asteroids.Weapons.Projectiles;
using Zenject;

namespace Asteroids.Weapons
{
    /// <summary>
    /// Binds what every weapon shares: the registry of projectiles in play and the handler that stops a
    /// projectile on its first hit, and the registry of laser beams and the handler that tells a beam what it
    /// touches. Both handlers are <see cref="ICollisionResponse"/>s, run in <see cref="CollisionResponseOrder"/>,
    /// so <see cref="CollisionInstaller"/> must be installed. A laser also needs the damage lookup of
    /// <c>DamageInstaller</c>.
    /// </summary>
    public sealed class WeaponsInstaller : Installer<WeaponsInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<ProjectileRegistry>().AsSingle();
            Container.Bind<ICollisionResponse>().To<ProjectileImpactHandler>().AsSingle();
            Container.BindInterfacesTo<LaserBeamRegistry>().AsSingle();
            Container.Bind<ICollisionResponse>().To<LaserContactHandler>().AsSingle();
        }
    }
}
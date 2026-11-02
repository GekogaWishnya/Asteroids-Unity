using Asteroids.Collisions;
using Asteroids.Weapons.Laser;
using Asteroids.Weapons.Projectiles;
using Zenject;

namespace Asteroids.Weapons
{
    /// <summary>
    /// Binds what every weapon shares: the registry of projectiles in play and the handler that stops a
    /// projectile on its first hit, and the registry of laser beams and the handler that tells a beam what it
    /// touches. Subscribes to <see cref="CollisionEntered"/> and <see cref="CollisionExited"/>, so
    /// <see cref="CollisionInstaller"/> must be installed. A laser also needs the damage lookup of
    /// <c>DamageInstaller</c>.
    /// </summary>
    public sealed class WeaponsInstaller : Installer<WeaponsInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<ProjectileRegistry>().AsSingle();

            Container.Bind<ProjectileImpactHandler>().AsSingle();
            Container.BindSignal<CollisionEntered>()
                .ToMethod<ProjectileImpactHandler>(handler => handler.OnCollisionEntered)
                .FromResolve();

            Container.BindInterfacesTo<LaserBeamRegistry>().AsSingle();

            Container.Bind<LaserContactHandler>().AsSingle();
            Container.BindSignal<CollisionEntered>()
                .ToMethod<LaserContactHandler>(handler => handler.OnCollisionEntered)
                .FromResolve();
            Container.BindSignal<CollisionExited>()
                .ToMethod<LaserContactHandler>(handler => handler.OnCollisionExited)
                .FromResolve();
        }
    }
}

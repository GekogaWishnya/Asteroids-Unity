using Asteroids.Collisions;
using Asteroids.Weapons.Projectiles;
using Zenject;

namespace Asteroids.Weapons
{
    /// <summary>
    /// Binds what every weapon shares: the registry of projectiles in play and the handler that stops a
    /// projectile on its first hit. Subscribes to <see cref="CollisionEntered"/>, so
    /// <see cref="CollisionInstaller"/> must be installed.
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
        }
    }
}

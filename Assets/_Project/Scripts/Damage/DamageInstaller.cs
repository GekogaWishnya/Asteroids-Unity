using Asteroids.Collisions;
using Zenject;

namespace Asteroids.Damage
{
    /// <summary>
    /// Requires <see cref="CollisionInstaller"/> to be installed first for <see cref="ContactDamageHandler"/>.
    /// </summary>
    public sealed class DamageInstaller : Installer<DamageInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<DamageRegistry>().AsSingle();

            Container.Bind<ContactDamageHandler>().AsSingle();
            Container.BindSignal<CollisionEntered>()
                .ToMethod<ContactDamageHandler>(handler => handler.OnCollisionEntered)
                .FromResolve();
            Container.BindSignal<CollisionExited>()
                .ToMethod<ContactDamageHandler>(handler => handler.OnCollisionExited)
                .FromResolve();
        }
    }
}

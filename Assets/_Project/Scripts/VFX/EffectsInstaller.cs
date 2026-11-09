using Asteroids.Core;
using Zenject;

namespace Asteroids.VFX
{
    /// <summary>
    /// Binds <see cref="EffectsDirector"/> (as <see cref="IEffectsPause"/>) and connects it to the gameplay
    /// signals. Requires the host installer to bind <see cref="Asteroids.Core.IConfigs.IEffectsConfig"/> and
    /// <see cref="IEffectFactory"/>, the simulation installer to bind <see cref="Asteroids.Common.IErrorLog"/>,
    /// and the player and enemies installers to declare <see cref="ShipDamaged"/>,
    /// <see cref="ShipDestroyed"/>, <see cref="EnemyDamaged"/> and <see cref="EnemyDied"/>.
    /// </summary>
    public sealed class EffectsInstaller : Installer<EffectsInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<EffectsDirector>().AsSingle();

            Container.BindSignal<ShipDamaged>()
                .ToMethod<EffectsDirector>(director => director.OnShipDamaged)
                .FromResolve();
            Container.BindSignal<ShipDestroyed>()
                .ToMethod<EffectsDirector>(director => director.OnShipDestroyed)
                .FromResolve();
            Container.BindSignal<EnemyDamaged>()
                .ToMethod<EffectsDirector>(director => director.OnEnemyDamaged)
                .FromResolve();
            Container.BindSignal<EnemyDied>()
                .ToMethod<EffectsDirector>(director => director.OnEnemyDied)
                .FromResolve();
        }
    }
}
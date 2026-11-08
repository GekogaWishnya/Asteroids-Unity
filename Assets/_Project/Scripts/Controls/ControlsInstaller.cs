using Asteroids.Core;
using Zenject;

namespace Asteroids.Controls
{
    /// <summary>
    /// Binds the player's input: <see cref="IInputSchemes"/>, <see cref="IMovementInput"/> and
    /// <see cref="IWeaponTriggers"/>, which read as idle unless <see cref="InputScheme.Gameplay"/> is active.
    /// <see cref="ShipInput"/> checks every axis at startup.
    /// </summary>
    public sealed class ControlsInstaller : Installer<ControlsInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<InputSchemes>().AsSingle();

            Container.Bind<IAxisReader>().To<UnityAxisReader>().AsSingle();
            Container.Bind<ShipInput>().AsSingle();
            Container.Bind<IInitializable>().To<ShipInput>().FromResolve();
            Container.BindInterfacesTo<SchemeGatedShipInput>().AsSingle();
        }
    }
}
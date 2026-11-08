using Asteroids.Core;
using NUnit.Framework;
using Zenject;

namespace Asteroids.Controls.Tests
{
    /// <summary>
    /// The game must read the scheme-gated input, never the raw one, and the raw one must still validate the
    /// Input Manager axes at startup.
    /// </summary>
    [TestFixture]
    public sealed class ControlsInstallerTests
    {
        private static DiContainer Install()
        {
            var container = new DiContainer();
            ControlsInstaller.Install(container);
            return container;
        }

        [Test]
        public void MovementAndTriggers_AreTheSchemeGatedInput()
        {
            var container = Install();

            Assert.That(container.Resolve<IMovementInput>(), Is.InstanceOf<SchemeGatedShipInput>());
            Assert.That(container.Resolve<IWeaponTriggers>(), Is.SameAs(container.Resolve<IMovementInput>()));
        }

        [Test]
        public void GatedInput_IsTicked()
        {
            var container = Install();

            Assert.That(container.ResolveAll<ITickable>(), Has.Some.InstanceOf<SchemeGatedShipInput>());
        }

        [Test]
        public void RawInput_IsInitializedToCheckTheAxes()
        {
            var container = Install();

            Assert.That(container.ResolveAll<IInitializable>(), Has.Some.InstanceOf<ShipInput>());
        }

        [Test]
        public void InputSchemes_IsASingleton()
        {
            var container = Install();

            Assert.That(container.Resolve<IInputSchemes>(), Is.SameAs(container.Resolve<IInputSchemes>()));
        }
    }
}

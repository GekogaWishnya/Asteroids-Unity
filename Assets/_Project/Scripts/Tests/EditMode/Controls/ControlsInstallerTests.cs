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

        /// <summary>
        /// The frame schedule ticks the gated input by its concrete type, so it must resolve to the very instance the
        /// game reads; a second instance would sample triggers nobody fires with.
        /// </summary>
        [Test]
        public void GatedInput_ResolvesByItsOwnTypeToTheSameInstance()
        {
            var container = Install();

            Assert.That(container.Resolve<SchemeGatedShipInput>(), Is.SameAs(container.Resolve<IMovementInput>()));
        }

        /// <summary>Nothing in Controls may tick itself any more; the frame schedule owns every tick.</summary>
        [Test]
        public void NothingIsAZenjectTickable()
        {
            var container = Install();

            Assert.That(container.ResolveAll<ITickable>(), Is.Empty);
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

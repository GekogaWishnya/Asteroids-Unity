using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Physics;
using NUnit.Framework;
using Zenject;

namespace Asteroids.Player.Tests
{
    /// <summary>
    /// Builds the ship through <see cref="PlayerInstaller"/>, with the real parts, and checks what only the
    /// whole ship can show: that the parts are there, in the right order, and leave before death is reported.
    /// </summary>
    [TestFixture]
    public sealed class ShipAssemblyTests
    {
        private const float Dt = 0.1f;

        private sealed class Rig
        {
            public readonly DiContainer Container = new();
            public readonly TestShipInput Input = new();
            public readonly TestCombatConfig Combat = new() { InvulnerabilitySeconds = 1.5f };
            public readonly FakeSimulationLoop Loop = new();
            public readonly FakeDamageRegistry Registry = new();
            public readonly FakeWeapon Weapon = new();
            public readonly FakeCollisionIgnore Collider = new();
            public readonly FakeProtectionView View = new();
            public readonly BodyPose Pose = new();
            public readonly Entity Entity;

            public Rig()
            {
                Entity = new Entity(Pose);

                SignalBusInstaller.Install(Container);
                Container.DeclareSignal<WaveStarted>();
                Container.BindInterfacesTo<TestShipInput>().FromInstance(Input);
                Container.Bind<IShipMovementConfig>().FromInstance(new TestMovementConfig());
                Container.Bind<IShipHealthConfig>().FromInstance(new TestHealthConfig { MaxHealth = 3 });
                Container.Bind<IShipRamConfig>().FromInstance(Combat);
                Container.Bind<IInvulnerabilityConfig>().FromInstance(Combat);
                Container.Bind<IWaveProtectionConfig>().FromInstance(Combat);
                Container.Bind<ISimulationLoop>().FromInstance(Loop);
                Container.Bind<IDamageRegistry>().FromInstance(Registry);
                Container.Bind<IWeapon>().FromInstance(Weapon);

                PlayerInstaller.Install(Container);

                // Creates the signal subscriptions, as a scene context would.
                Container.ResolveRoots();
            }

            public PlayerHealth Health => Container.Resolve<PlayerHealth>();

            public IShipController CreateShip() =>
                Container.Resolve<ShipControllerFactory>()
                    .Create(Entity, new FakeProtectionTargets(new[] { Collider }, new[] { View }));
        }

        [Test]
        public void Enable_AttachesEveryPart()
        {
            var rig = new Rig();

            rig.CreateShip().Enable();

            Assert.That(rig.Loop.LiveCount(SimulationPhase.Input), Is.EqualTo(1));
            Assert.That(rig.Loop.LiveCount(SimulationPhase.Motion), Is.EqualTo(1));
            Assert.That(rig.Loop.LiveCount(SimulationPhase.Logic), Is.EqualTo(1));
            Assert.That(rig.Registry.LiveDamageables, Is.EqualTo(1));
            Assert.That(rig.Registry.LiveSources, Is.EqualTo(1));
            Assert.That(rig.Weapon.LiveCount, Is.EqualTo(1));
            Assert.That(rig.Weapon.Attachments[0].Carrier, Is.SameAs(rig.Entity));
            Assert.That(rig.Weapon.Attachments[0].Trigger, Is.SameAs(rig.Input));
        }

        [Test]
        public void Enable_AttachesMovementThenDefenseThenRamThenArmament()
        {
            var rig = new Rig();
            var order = new System.Collections.Generic.List<string>();
            rig.Registry.OnRegister = kind => order.Add(kind);
            rig.Loop.OnRegister = phase => order.Add(phase.ToString());
            rig.Weapon.OnAttach = () => order.Add("Weapon");

            rig.CreateShip().Enable();

            Assert.That(order, Is.EqualTo(new[] { "Input", "Motion", "Logic", "Damageable", "Source", "Weapon" }));
        }

        [Test]
        public void Damage_ThroughTheRegistration_HurtsThePlayerHealth()
        {
            var rig = new Rig();
            rig.CreateShip().Enable();

            rig.Registry.Damageables[0].Item.TakeDamage(1);

            Assert.That(rig.Health.Current.Value, Is.EqualTo(2));
        }

        [Test]
        public void Damage_Lethal_DetachesEverythingBeforeReportingIt()
        {
            var rig = new Rig();
            var liveWhenReported = -1;
            CollisionLayer? ignoredWhenReported = null;
            int? viewCallsWhenReported = null;
            rig.Container.Resolve<SignalBus>().Subscribe<ShipDestroyed>(_ =>
            {
                liveWhenReported = rig.Loop.LiveCount(SimulationPhase.Input) +
                                   rig.Loop.LiveCount(SimulationPhase.Motion) +
                                   rig.Loop.LiveCount(SimulationPhase.Logic) +
                                   rig.Registry.LiveDamageables + rig.Registry.LiveSources +
                                   rig.Weapon.LiveCount;
                ignoredWhenReported = rig.Collider.Ignored;
                viewCallsWhenReported = rig.View.Calls.Count;
            });
            rig.CreateShip().Enable();

            rig.Registry.Damageables[0].Item.TakeDamage(3);

            Assert.That(liveWhenReported, Is.EqualTo(0));
            Assert.That(ignoredWhenReported, Is.EqualTo(CollisionLayer.None));
            Assert.That(viewCallsWhenReported, Is.EqualTo(2));
            Assert.That(rig.View.Calls, Is.EqualTo(new[] { true, false }));
        }

        [Test]
        public void Tick_AfterDestruction_DoesNotMoveOrTurn()
        {
            var rig = new Rig();
            rig.Input.Turn = 1f;
            rig.Input.Thrust = 1f;
            rig.CreateShip().Enable();
            rig.Health.TakeDamage(3);

            rig.Loop.Tick(Dt);

            Assert.That(rig.Pose.Angle, Is.EqualTo(0f));
            Assert.That(rig.Pose.Position, Is.EqualTo(UnityEngine.Vector2.zero));
        }

        [Test]
        public void WaveStarted_Signal_ProtectsTheShip()
        {
            var rig = new Rig();
            rig.CreateShip().Enable();

            rig.Container.Resolve<SignalBus>().Fire(new WaveStarted(1, 3));
            rig.Registry.Damageables[0].Item.TakeDamage(1);

            Assert.That(rig.Health.Current.Value, Is.EqualTo(3));
        }
    }
}

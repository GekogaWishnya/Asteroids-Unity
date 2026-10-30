using System;
using Asteroids.Common;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Invulnerability;
using Asteroids.Physics;

namespace Asteroids.Player
{
    /// <summary>
    /// <para>
    /// Assembles the behavior of a ship.
    /// Creates and registers <see cref="ShipControlStep"/> in <see cref="SimulationPhase.Input"/> and
    /// <see cref="MotionStep"/> in <see cref="SimulationPhase.Motion"/>, which share one <see cref="InertialMotion"/>.
    /// </para>
    /// <para>
    /// While enabled, the ship is also registered as a damageable and as a damage source (it hurts what it
    /// rams), and it listens to the depletion of the player's health. The damageable is an
    /// <see cref="InvulnerabilityGuard"/> around the player's health, meaning each hit that gets through, and each wave
    /// start (through <see cref="WaveStartProtection"/>) start an invulnerability window for the ship.
    /// </para>
    /// <para>
    /// When health runs out, the controller disables itself first, which ends the protection, restores the
    /// colliders, tells the views and takes the weapon off, and then reports <see cref="IShipEventSink.Destroyed"/>.
    /// A dead ship cannot be enabled until its health is reset.
    /// </para>
    /// </summary>
    public sealed class ShipController : IShipController
    {
        private readonly RegistrationScope _registrations;
        private readonly PlayerHealth _health;
        private readonly IShipEventSink _events;
        private readonly IEntity _entity;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        /// <exception cref="ArgumentException">A list of <paramref name="targets"/> contains null.</exception>
        public ShipController(
            IMovementInput input,
            IFireInput fireInput,
            IShipMovementConfig config,
            IShipRamConfig ram,
            IInvulnerabilityConfig invulnerability,
            ISimulationLoop loop,
            IEntity entity,
            IProtectionTargets targets,
            WaveStartProtection waveProtection,
            PlayerHealth health,
            IDamageRegistry damageRegistry,
            IWeapon weapon,
            IShipEventSink events)
        {
            if (fireInput == null) throw new ArgumentNullException(nameof(fireInput));
            if (ram == null) throw new ArgumentNullException(nameof(ram));
            if (loop == null) throw new ArgumentNullException(nameof(loop));
            if (waveProtection == null) throw new ArgumentNullException(nameof(waveProtection));
            if (damageRegistry == null) throw new ArgumentNullException(nameof(damageRegistry));
            if (weapon == null) throw new ArgumentNullException(nameof(weapon));

            _entity = entity ?? throw new ArgumentNullException(nameof(entity));
            _health = health ?? throw new ArgumentNullException(nameof(health));
            _events = events ?? throw new ArgumentNullException(nameof(events));

            var pose = entity.Pose;
            var motion = new InertialMotion(config);
            var controlStep = new ShipControlStep(input, config, pose, motion);
            var motionStep = new MotionStep(pose, motion);
            var guard = new InvulnerabilityGuard(health, invulnerability, targets);
            var source = new ShipRamSource(ram);

            _registrations = new RegistrationScope(new Func<IDisposable>[]
            {
                () => loop.Register(SimulationPhase.Input, controlStep),
                () => loop.Register(SimulationPhase.Motion, motionStep),
                () => guard.Attach(loop),
                () => waveProtection.Attach(guard),
                () => damageRegistry.RegisterDamageable(entity, guard),
                () => damageRegistry.RegisterSource(entity, source),
                () => weapon.Attach(entity, fireInput),
                ListenForDepletion
            });
        }

        /// <summary>Does nothing while the player's health is depleted.</summary>
        public void Enable()
        {
            if (_health.IsDead) return;

            _registrations.Enable();
        }

        public void Disable() => _registrations.Disable();

        private IDisposable ListenForDepletion()
        {
            _health.Depleted += OnDepleted;
            return new DepletionSubscription(_health, OnDepleted);
        }

        private void OnDepleted()
        {
            Disable();
            _events.Destroyed(_entity);
        }

        private sealed class DepletionSubscription : IDisposable
        {
            private readonly PlayerHealth _health;
            private readonly Action _handler;

            public DepletionSubscription(PlayerHealth health, Action handler)
            {
                _health = health;
                _handler = handler;
            }

            public void Dispose() => _health.Depleted -= _handler;
        }
    }
}

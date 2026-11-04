using System;
using Asteroids.Common;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Damage;
using UnityEngine;

namespace Asteroids.Weapons.Laser
{
    /// <summary>A beam that fires for as long as the trigger is held and the tank has energy.</summary>
    public sealed class LaserWeapon : IWeapon, ILaserStatus
    {
        private readonly ILaserWeaponConfig _config;
        private readonly ISimulationLoop _loop;
        private readonly IPooledView _beam;
        private IDisposable _beamColliders;
        private readonly ILaserBeamRegistry _beams;
        private readonly LaserEnergy _energy;
        private readonly LaserHitTable _hits;
        private Attachment _attachment;

        /// <exception cref="ArgumentNullException">Any argument (or the beam's entity) is null.</exception>
        public LaserWeapon(
            ILaserWeaponConfig config,
            ISimulationLoop loop,
            IPooledView beam,
            ILaserBeamRegistry beams,
            IDamageLookup damage)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _loop = loop ?? throw new ArgumentNullException(nameof(loop));
            _beam = beam ?? throw new ArgumentNullException(nameof(beam));
            _beams = beams ?? throw new ArgumentNullException(nameof(beams));
            if (damage == null) throw new ArgumentNullException(nameof(damage));
            if (beam.Entity == null) throw new ArgumentNullException(nameof(beam), "The beam has no entity.");

            _energy = new LaserEnergy(config);
            _hits = new LaserHitTable(config, damage);
        }

        public IObservableValue<float> Energy => _energy.Energy;
        public IObservableValue<float> SecondsToFull => _energy.SecondsToFull;

        public bool IsBeamOn { get; private set; }

        public IDisposable Attach(IEntity carrier, IFireInput trigger)
        {
            if (carrier == null) throw new ArgumentNullException(nameof(carrier));
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));
            if (_attachment != null) throw new InvalidOperationException("The weapon is already attached.");

            var pose = carrier.Pose ?? throw new ArgumentNullException(nameof(carrier), "The carrier has no pose.");
            var contacts = new BeamContacts(_hits);

            var attachment = new Attachment(this, new RegistrationScope(new Func<IDisposable>[]
            {
                () => _beams.Register(_beam.Entity, contacts),
                () => _loop.Register(SimulationPhase.Input, new TriggerStep(this, pose, trigger)),
                () => _loop.Register(SimulationPhase.Motion, new AimStep(this, pose)),
                () => _loop.Register(SimulationPhase.Logic, new HitStep(_hits)),
                () => new DisposableAction(Shutdown)
            }));

            attachment.Scope.Enable();
            return _attachment = attachment;
        }

        private void UpdateTrigger(IPose pose, float deltaTime, bool triggerHeld)
        {
            var on = _energy.Tick(deltaTime, triggerHeld);
            if (on == IsBeamOn) return;

            if (on) TurnOn(pose);
            else TurnOff();
        }

        private void TurnOn(IPose pose)
        {
            var (position, angle) = Muzzle(pose);
            IsBeamOn = true;

            try
            {
                _beam.Show(position, angle);
                _beamColliders = _beam.RegisterColliders();
            }
            catch
            {
                IsBeamOn = false;
                try
                {
                    _beam.Hide();
                }
                catch
                {
                    // The failure that turned the beam off matters more.
                }

                _energy.Interrupt();
                throw;
            }
        }

        private void TurnOff()
        {
            if (!IsBeamOn) return;

            IsBeamOn = false;
            var colliders = _beamColliders;
            _beamColliders = null;
            try
            {
                colliders?.Dispose();
            }
            finally
            {
                try
                {
                    _beam.Hide();
                }
                finally
                {
                    _hits.Clear();
                }
            }
        }

        private void Aim(IPose pose)
        {
            if (!IsBeamOn) return;

            var (position, angle) = Muzzle(pose);
            var beamPose = _beam.Entity.Pose;
            beamPose.Position = position;
            beamPose.Angle = angle;
        }

        private (Vector2 position, float angle) Muzzle(IPose pose) =>
            (pose.Position + pose.Up * _config.MuzzleOffset, pose.Angle);

        private void Shutdown()
        {
            try
            {
                TurnOff();
            }
            finally
            {
                _energy.Interrupt();
            }
        }

        private sealed class BeamContacts : ILaserBeamContacts
        {
            private readonly LaserHitTable _hits;

            public BeamContacts(LaserHitTable hits) => _hits = hits;

            public void Entered(IEntity target) => _hits.Enter(target);
            public void Exited(IEntity target) => _hits.Exit(target);
        }

        private sealed class TriggerStep : ISimulationStep
        {
            private readonly LaserWeapon _weapon;
            private readonly IPose _pose;
            private readonly IFireInput _trigger;

            public TriggerStep(LaserWeapon weapon, IPose pose, IFireInput trigger)
            {
                _weapon = weapon;
                _pose = pose;
                _trigger = trigger;
            }

            public void Tick(float deltaTime) => _weapon.UpdateTrigger(_pose, deltaTime, _trigger.IsFiring);
        }

        private sealed class AimStep : ISimulationStep
        {
            private readonly LaserWeapon _weapon;
            private readonly IPose _pose;

            public AimStep(LaserWeapon weapon, IPose pose)
            {
                _weapon = weapon;
                _pose = pose;
            }

            public void Tick(float deltaTime) => _weapon.Aim(_pose);
        }

        private sealed class HitStep : ISimulationStep
        {
            private readonly LaserHitTable _hits;

            public HitStep(LaserHitTable hits) => _hits = hits;

            public void Tick(float deltaTime) => _hits.Tick(deltaTime);
        }

        private sealed class Attachment : IDisposable
        {
            private readonly LaserWeapon _owner;
            private bool _disposed;

            public Attachment(LaserWeapon owner, RegistrationScope scope)
            {
                _owner = owner;
                Scope = scope;
            }

            public RegistrationScope Scope { get; }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;

                try
                {
                    Scope.Dispose();
                }
                finally
                {
                    if (ReferenceEquals(_owner._attachment, this)) _owner._attachment = null;
                }
            }
        }
    }
}
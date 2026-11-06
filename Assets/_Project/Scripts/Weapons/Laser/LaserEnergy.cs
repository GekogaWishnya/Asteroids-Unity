using System;
using Asteroids.Common;
using Asteroids.Core;
using Asteroids.Core.IConfigs;

namespace Asteroids.Weapons.Laser
{
    /// <summary>
    /// <para>
    /// The laser's tank and the rules of its trigger. The tank starts empty. A press starts the beam when the tank
    /// holds at least <see cref="ILaserWeaponConfig.MinEnergyToFire"/> and the beam stays on while the trigger is
    /// held and the tank is not empty.
    /// </para>
    /// <para>
    /// The tank does not refill while the beam is on. It starts refilling
    /// <see cref="ILaserWeaponConfig.RefillDelay"/> seconds after the beam stops.
    /// </para>
    /// </summary>
    public sealed class LaserEnergy : ILaserStatus
    {
        private readonly ILaserWeaponConfig _config;
        private readonly ObservableValue<float> _fraction = new(0f);
        private readonly ObservableValue<float> _secondsToFull = new(0f);
        private float _refillWait;
        private bool _wasHeld;

        /// <exception cref="ArgumentNullException"><paramref name="config"/> is null.</exception>
        public LaserEnergy(ILaserWeaponConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            Publish();
        }

        public float Current { get; private set; }

        public bool IsFiring { get; private set; }

        public IObservableValue<float> Energy => _fraction;
        public IObservableValue<float> SecondsToFull => _secondsToFull;

        /// <summary>Advances the tank by <paramref name="deltaTime"/> seconds.</summary>
        /// <param name="triggerHeld">Whether the trigger is held on this tick.</param>
        /// <returns>True when the beam is on after this tick.</returns>
        public bool Tick(float deltaTime, bool triggerHeld)
        {
            var pressed = triggerHeld && !_wasHeld;
            _wasHeld = triggerHeld;

            if (IsFiring)
            {
                if (triggerHeld) Drain(deltaTime);
                else Stop();
            }
            else if (pressed && Current >= _config.MinEnergyToFire)
            {
                IsFiring = true;
                Drain(deltaTime);
            }
            else
            {
                Refill(deltaTime);
            }

            Publish();
            return IsFiring;
        }

        /// <summary>Empties the tank and forgets the trigger.</summary>
        public void Reset()
        {
            Current = 0f;
            IsFiring = false;
            _refillWait = 0f;
            _wasHeld = false;
            Publish();
        }

        /// <summary>Stops the beam without a release.</summary>
        public void Interrupt()
        {
            if (!IsFiring) return;

            Stop();
            _wasHeld = true;
            Publish();
        }

        private void Drain(float deltaTime)
        {
            Current -= _config.DrainPerSecond * deltaTime;
            if (Current > 0f) return;

            Current = 0f;
            Stop();
        }

        private void Stop()
        {
            IsFiring = false;
            _refillWait = _config.RefillDelay;
        }

        private void Refill(float deltaTime)
        {
            var refillTime = deltaTime;
            if (_refillWait > 0f)
            {
                var waited = Math.Min(_refillWait, deltaTime);
                _refillWait -= waited;
                refillTime -= waited;
            }

            if (refillTime > 0f)
                Current = Math.Min(_config.MaxEnergy, Current + _config.RefillPerSecond * refillTime);
        }

        private void Publish()
        {
            var max = _config.MaxEnergy;
            _fraction.Value = max > 0f ? Math.Clamp(Current / max, 0f, 1f) : 0f;

            var missing = max - Current;
            if (missing <= 0f)
            {
                _secondsToFull.Value = 0f;
                return;
            }

            var wait = IsFiring ? _config.RefillDelay : _refillWait;
            _secondsToFull.Value = wait + missing / _config.RefillPerSecond;
        }
    }
}
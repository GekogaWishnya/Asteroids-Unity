using System;

namespace Asteroids.Weapons
{
    /// <summary>
    /// Spaces the shots of a held trigger. The first shot is immediate, then one per interval for as long as the
    /// trigger is held. Releasing the trigger does not end the wait, so tapping is never faster than holding.
    /// </summary>
    public sealed class FireCooldown
    {
        private float _remaining;
        public bool IsReady => _remaining <= 0f;

        /// <summary>Advances the wait by <paramref name="deltaTime"/>.</summary>
        /// <param name="interval">Seconds between two shots. Read on every tick, so it may change at runtime.</param>
        /// <returns>True when a shot is due on this tick.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="interval"/> is not a positive number.</exception>
        public bool Tick(float deltaTime, bool triggerHeld, float interval)
        {
            if (!float.IsFinite(interval) || interval <= 0f)
                throw new ArgumentOutOfRangeException(nameof(interval), "Fire interval must be a positive number.");

            if (_remaining > 0f) _remaining -= deltaTime;

            if (!triggerHeld)
            {
                if (_remaining < 0f) _remaining = 0f;
                return false;
            }

            if (_remaining > 0f) return false;

            // Carry the overshoot, but never more than a whole interval.
            _remaining = Math.Max(_remaining + interval, 0f);
            return true;
        }
    }
}
